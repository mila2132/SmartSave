import os
import paho.mqtt.client as mqtt
import time
import random


broker = os.getenv('MQTT_BROKER', 'localhost')
port = int(os.getenv('MQTT_PORT', 1883))
seconds = int(os.getenv('PUBLISH_INTERVAL', 3600))
topic = "nest/temperature"


def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("Connected to MQTT Broker!")
    else:
        print(f"Failed to connect, return code {rc}")

client = mqtt.Client()
client.on_connect = on_connect

client.connect(broker, port, 60)

def publish_temperature():
    base_temp = 22.0  
    while True:
        temp_change = random.uniform(-0.5, 0.5)
        current_temp = base_temp + temp_change
        client.publish(topic, f"{current_temp:.2f}")
        print(f"Published temperature: {current_temp:.2f}°C to {topic}")
        time.sleep(seconds)

try:
    publish_temperature()
except KeyboardInterrupt:
    client.disconnect()
    print("Disconnected from MQTT Broker.")
