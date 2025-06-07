from flask import Flask, request, jsonify
from dotenv import load_dotenv
import schedule
import time
import requests
import os
from threading import Thread

load_dotenv()

app = Flask(__name__)

active = False
temperature = 0
thermostatMode = ''
light_data_by_hour = {}
url_service = os.getenv('URL_SERVICE')


@app.route('/receiveData', methods=['POST'])
def receive_data():
    global light_data_by_hour, temperature, thermostatMode, active
    data = request.json
    temperature = request.json.get('temperature')
    thermostatMode = request.json.get('thermostatMode')
    light_data_by_hour = data.get('lightData', {})
    active = True
    
    check_hourly_data()
    if not schedule.jobs:
        schedule.every().hour.at(":00").do(check_hourly_data)
        print("Scheduled job to check data every hour.")

    return jsonify({'result': True, 'message': 'Data received and automatic mode scheduled'}), 200


def check_hourly_data():
    global active, url_service, light_data_by_hour
    if not active:
        print("Automatic mode is disabled.")
        return

    current_hour = time.localtime().tm_hour        
    current_hour_str = str(current_hour)
    current_hour_data = light_data_by_hour[current_hour_str]
    label = current_hour_data.get('label')

    if label == 'expensive':
        requests.post(url_service + '/turnOff', json={ 'isAutomaticService': True})
        print(f"TurnOff signal sent for hour {current_hour}")
    elif label == 'acceptable':
        # Enviar petición para actualizar temperatura
        requests.post(url_service + '/updateTemperature', json={'temperature': temperature, 'thermostatMode': thermostatMode, 'isAutomaticService': True})
        print(f"Temperature update sent for hour {current_hour}")


@app.route('/controlMode', methods=['POST'])
def set_automatic_mode():
    global active
    mode = request.json.get('mode')  
    active = mode
    return jsonify({'result': True, 'message': f"Automatic mode set to {active}"}), 200


def run_scheduler():
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == '__main__':
    t = Thread(target=run_scheduler)
    t.start() 
    app.run(host='127.0.0.1', port=5001, debug=True)  
