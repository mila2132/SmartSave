import os
import json
import requests
import pandas as pd 
import numpy as np
import boto3

def get_data_from_api(api_url, headers_request):
    response = requests.get(api_url, headers=headers_request)
    response.raise_for_status()
    return response.json().get("PVPC")
    
def upload_to_s3(bucket_name, s3_file_key, content):
    s3_client = boto3.client('s3')
    s3_client.put_object(Bucket=bucket_name, Key=s3_file_key, Body=content)
    
def classify_price_percetile(price, cheap_threshold, expensive_threshold):
    if price <= cheap_threshold:
        return 'cheap'
    elif price <= expensive_threshold:
        return 'acceptable'
    else:
        return 'expensive'
    
def process_data(data_received):
    df = pd.DataFrame(data_received)
    df['Hora'] = df['Hora'].apply(lambda hora: int(hora.split('-')[0]))
    df['PCB'] = df['PCB'].str.replace(',', '.').astype(float)
    df['PCB'] = round(df['PCB'] / 1000, 3)
    prices = df['PCB'].values
    cheap_threshold = np.percentile(prices, 33)
    expensive_threshold = np.percentile(prices, 66)
    df['Etiqueta'] = df['PCB'].apply(lambda price: classify_price_percetile(price, cheap_threshold, expensive_threshold))
    df_processed = df[['Dia', 'Hora', 'PCB', 'Etiqueta']].rename(columns={'Dia': 'day', 'Hora': 'hour', 'PCB': 'price', 'Etiqueta': 'label'})
    return df_processed.to_json(orient='records')

    
def lambda_handler(event, context):
    api_url = os.getenv('URL_API_ESIOS')
    headers = json.loads(os.getenv('HEADERS'))
    bucket_name = os.environ.get('BUCKET_NAME')
    s3_file_key = 'pvpc-actual/data_today.json'
    
    try:
        # Obtener los datos de la API
        data_received = get_data_from_api(api_url, headers)

        # Convertir el contenido a una cadena de texto JSON
        data_processed = process_data(data_received)
        content_str = json.dumps(json.loads(data_processed))

         
        # Subir los datos a S3
        upload_to_s3(bucket_name, s3_file_key, content_str)
        
    except requests.RequestException as e:
        return {'statusCode': 500, 'body': json.dumps('Error al obtener datos de la API')}
    except boto3.exceptions.Boto3Error as e:
        return {'statusCode': 500, 'body': json.dumps('Error al interactuar con S3')}
    
    return {'statusCode': 200, 'body': json.dumps('Proceso completado con exito')}
