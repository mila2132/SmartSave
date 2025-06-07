import boto3
import requests
from flask import Flask, request, jsonify
from dotenv import load_dotenv
import os
import mysql.connector
import json
import schedule
import time
from threading import Thread

load_dotenv()

app = Flask(__name__)

light_data_by_hour = {}

db_config = {
    'user': os.getenv('DB_USER'),
    'password': os.getenv('DB_PASSWORD'),
    'host': os.getenv('DB_HOST'),
    'database': os.getenv('DB_NAME')
}
automatic_mode = False
temperature = 0
thermostatMode = ''
url_service_automatic = os.getenv('URL_SERVICE_AUTOMATIC')
s3_client = None
isAutomaticService = False

def create_s3_client():
    global s3_client
    cognito_identity_client = boto3.client('cognito-identity', region_name=os.getenv('AWS_REGION'))

    identity_id_response = cognito_identity_client.get_id(
        IdentityPoolId=os.getenv('COGNITO_IDENTITY_POOL_ID')
    )
    identity_id = identity_id_response['IdentityId']

    credentials_response = cognito_identity_client.get_credentials_for_identity(
        IdentityId=identity_id
    )
    credentials = credentials_response['Credentials']

    s3_client = boto3.client(
        's3',
        aws_access_key_id=credentials['AccessKeyId'],
        aws_secret_access_key=credentials['SecretKey'],
        aws_session_token=credentials['SessionToken']
    )


def fetch_data_from_s3():
    global light_data_by_hour, s3_client
    response = s3_client.get_object(Bucket=os.getenv('S3_BUCKET_NAME'), Key=os.getenv('S3_KEY'))
    content = response['Body'].read().decode('utf-8')
    data = json.loads(content)
    light_data_by_hour = {item['hour']: item for item in data}

schedule.every().day.at("00:00").do(fetch_data_from_s3)

@app.route('/auth', methods=['POST'])
def authenticate():
    email = request.json.get('email')
    if not email:
        return jsonify({'result': False, 'message': 'Email is required'}), 400

    try:
        cnx = mysql.connector.connect(**db_config)
        cursor = cnx.cursor()
        query = "SELECT 1 FROM credential WHERE email = %s"
        cursor.execute(query, (email,))
        result = cursor.fetchone()
        cursor.close()
        cnx.close()
        return jsonify({'result': bool(result)}), 200 if result else 404
    except Exception as e:
        return jsonify({'result': False, 'message': str(e)}), 500


@app.route('/modeAutomatic', methods=['POST'])
def active_automatic_mode():
    global automatic_mode, url_service_automatic, light_data_by_hour
    automatic_mode = True
    temperature = request.json.get('temperature')
    thermostatMode = request.json.get('thermostatMode')

    data_to_send = {
        'temperature': temperature,
        'thermostatMode': thermostatMode,
        'lightData': light_data_by_hour
    }

    # Enviar la petición POST al servicio de automatización
    try:
        response = requests.post(url_service_automatic + '/receiveData', json=data_to_send)
        data = response.json()
        responseService = data['message']
        if response.status_code == 200:
            return jsonify({'result': True, 'message': 'Automatic mode activated and data sent', 'messageAutomaticService': responseService}), 200
        else:
            return jsonify({'result': False, 'message': 'Failed to send data to other service'}), 500
    except requests.exceptions.RequestException as e:
        return jsonify({'result': False, 'message': str(e)}), 500


@app.route('/modeManual', methods=['GET'])
def manual_mode():
    global automatic_mode, url_service_automatic
    automatic_mode = False
    try:
        response = requests.post(url_service_automatic + '/controlMode', json={'mode': False})
        data = response.json()
        responseService = data['message']
        if response.status_code == 200:
            return jsonify({'result': True, 'message': 'Manual mode activated', 'messageAutomaticService' : responseService }), 200
        else:
            return jsonify({'result': False, 'message': 'Failed to send data to other service'}), 500
    except requests.exceptions.RequestException as e:
        return jsonify({'result': False, 'message': str(e)}), 500


@app.route('/updateTemperature', methods=['POST'])
def process_data():
    global thermostatMode, isAutomaticService, automatic_mode
    temperature = request.json.get('temperature')
    thermostatMode = request.json.get('thermostatMode')
    isAutomaticService = request.json.get('isAutomaticService', False) 
    
    if not automatic_mode or isAutomaticService:
        if thermostatMode == 'Cool':
            # Enviar petición a la API de Google Nest Thermostat para actualizar la temperatura de acuerdo al thermostatMode
            return jsonify({'result': True, 'message': 'Temperature updated'}), 200
        elif thermostatMode == 'Heat':
            # Enviar petición a la API de Google Nest Thermostat para actualizar la temperatura de acuerdo al thermostatMode
            return jsonify({'result': True, 'message': 'Temperature updated'}), 200
        else:
            return jsonify({'result': False, 'message': 'Invalid data'}), 400
    else:
        isAutomaticService = False
        return jsonify({'result': False, 'message': 'Automatic mode is enabled'}), 403
    
    
@app.route('/turnOff', methods=['GET', 'POST'])
def turn_off():
    global isAutomaticService, automatic_mode
    if request.method == 'POST':    
        isAutomaticService = request.json.get('isAutomaticService')
        
    if not automatic_mode or isAutomaticService:
        isAutomaticService = False
        return jsonify({'result': True, 'message': 'Thermostat turned off'}), 200
    else:
        isAutomaticService = False
        return jsonify({'result': False, 'message': 'Automatic mode is enabled'}), 403
    
    
def run_schedule():
    while True:
        schedule.run_pending()
        time.sleep(1)    
    
if __name__ == '__main__':
    create_s3_client()
    fetch_data_from_s3()
    t = Thread(target=run_schedule)
    t.start()
    app.run(host='127.0.0.1', port=5000, debug=True)
