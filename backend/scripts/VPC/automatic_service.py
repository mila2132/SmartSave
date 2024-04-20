from flask import Flask, request, jsonify
import schedule
import time
import requests
import os
from threading import Thread


app = Flask(__name__)

active = True
temperature = 0
thermostatMode = ''
light_data_by_hour = {}
url_service = os.getenv('URL_SERVICE')

@app.route('/receiveData', methods=['POST'])
def receive_data():
    global light_data_by_hour, temperature, thermostatMode
    data = request.json
    temperature = request.json.get('temperature')
    thermostatMode = request.json.get('thermostatMode')
    light_data_by_hour = data.get('lightData', {})

    if not schedule.jobs:
        schedule.every().hour.at(":00").do(check_hourly_data)
        print("Scheduled job to check data every hour.")

    return jsonify({'ok': True, 'message': 'Data received and automatic mode scheduled'}), 200


@app.route('/updateDataModeAutomatic', methods=['POST'])
def update_data_mode_automatic():
    global temperature, thermostatMode, light_data_by_hour
    temperature = request.json.get('temperature')
    thermostatMode = request.json.get('thermostatMode')
    light_data_by_hour = request.json.get('lightData')
    return jsonify({'ok': True, 'message': 'Data updated'}), 200


def check_hourly_data():
    global active
    if not active:
        print("Automatic mode is disabled.")
        return

    current_hour = time.localtime().tm_hour
    current_hour_data = light_data_by_hour[current_hour]
    label = current_hour_data.get('label')

    if label == 'expensive':
        requests.post(url_service + '/turnOff')
        print(f"TurnOff signal sent for hour {current_hour}")
    elif label == 'acceptable':
        # Enviar petición para actualizar temperatura
        requests.post(url_service + '/updateTemperature', json={'temperature': temperature, 'thermostatMode': thermostatMode})
        print(f"Temperature update sent for hour {current_hour}")


@app.route('/controlMode', methods=['POST'])
def set_automatic_mode():
    global active
    mode = request.json.get('mode')  
    active = mode
    return jsonify({'ok': True, 'message': f"Automatic mode set to {active}"}), 200


def run_scheduler():
    while True:
        schedule.run_pending()
        time.sleep(1)

if __name__ == '__main__':
    t = Thread(target=run_scheduler)
    t.start() 
    app.run(host='127.0.0.1', port=5001, debug=True)  
