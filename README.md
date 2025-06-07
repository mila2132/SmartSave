
# SmartSave

SmartSave es una aplicación móvil diseñada para ayudar a las personas adultas o mayores a ahorrar energía regulando de manera inteligente el termostato de sus hogares. La aplicación utiliza tecnología avanzada para monitorear los precios de la electricidad de cada día y ajustar automáticamente la temperatura del termostato para optimizar el consumo de energía y reducir los costos.



## Tecnologias  
- Docker
- Python
- Ngrok o Port forwarding de Visual Studio Code



## Backend

Levantar los dos servicios dentro de la carpeta backend/scripts

### VPC
En la carpeta VPC.

Crear Base de datos con el script creacion_database.sql.

Instalar dependencias
```bash
  pip install -r requirements.txt
```

Crear las siguiente variables de entorno

`DB_USER`

`DB_PASSWORD`

`DB_HOST`

`DB_NAME`

`AWS_REGION`=''

`COGNITO_IDENTITY_POOL_ID`=''

`S3_BUCKET_NAME`=pvpc

`S3_KEY`=pvpc-actual/data_today.json

`URL_SERVICE_AUTOMATIC`=http://localhost:5001

`URL_SERVICE`=http://localhost:5000


Ejecutar ambos scripts

```bash
  python automatic_service.py
  python google_nest_thermostat_service.pycd my-project
```

Ngrok o Port forwarding de VSCode del puerto 5000, unico servicio expuesto, para generar una URI publica necesario para el frontend. (Ejemplo: https://64q6mw0r-5000.uks1.devtunnels.ms)

### Docker

En de la carpeta Docker.

Construir la imagen
```bash
docker build -t nest-publisher -f Dockerfile_python . 
```

Ejecutar la imagen
```bash
docker run -d --name nest-publisher nest-publisher 
```



## Frontend

Editar appsettings.json


```javascript
  {
	"AWS": {
		"CognitoPoolId": "us-east-1:53701935-4dbf-4d23-890e-dc02f496e1d9",
		"Region": "us-east-1",
		"BucketS3": [
			{
				"BucketName": "pvpc",
				"Key": "pvpc-actual/data_today.json"
			}
		]
	},
	"API": {
		"GoogleNestThermostat": "URI publica generada en el backend"
	},
	"GoogleNest": {
		"broker": "test.mosquitto.org"
	}
}
```

