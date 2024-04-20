-- Crear la base de datos
CREATE DATABASE IF NOT EXISTS ThermostatDB;
USE ThermostatDB;

-- Crear la tabla 'credential'
CREATE TABLE IF NOT EXISTS credential (
    email VARCHAR(255) NOT NULL, 
    projectid CHAR(36) NOT NULL,
    deviceid CHAR(36) NOT NULL,
    token VARCHAR(255) NOT NULL
);

-- Mostrar las tablas para verificar que se haya creado correctamente
SHOW TABLES;

-- Insertar datos para pruebas
INSERT INTO credential (email, projectid, deviceid, token) VALUES
('user1@example.com', UUID(), UUID(), 'token12345'),
('user2@example.com', UUID(), UUID(), 'token67890'),
('user3@example.com', UUID(), UUID(), 'tokenabcde');

 