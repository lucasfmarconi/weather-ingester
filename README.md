# Weather Data Ingester

This repository contains a C# application for ingesting data from an MQTT broker and storing it in an InfluxDB data series.

## Overview

The MQTT to InfluxDB Data Ingester is designed to seamlessly capture data from an MQTT broker and store it in an InfluxDB database. This is particularly useful for scenarios where real-time data ingestion is required.

## Features

- **MQTT Integration**: Connects to MQTT brokers to receive data streams.
- **InfluxDB Integration**: Stores received data into InfluxDB, maintaining data integrity.
- 
## Prerequisites

Before running the application, ensure you have the following installed:

- .NET Core SDK
- InfluxDB instance accessible from the application
- Access to an MQTT broker with data streams to ingest

## Getting Started

1. Clone this repository:

    ```bash
    git clone https://github.com/lucasfmarconi/weather-ingester.git
    ```

2. Navigate to the project directory:

    ```bash
    cd weather-ingester
    ```

3. Configure the application settings in `appsettings.json`:

    ```json
    {
      "MQTT": {
        "BrokerAddress": "mqtt://your-mqtt-broker-address",
        "Topic": "your/mqtt/topic"
      },
      "InfluxDB": {
        "Server": "http://your-influxdb-server",
        "Database": "your-influxdb-database",
        "Username": "your-influxdb-username",
        "Password": "your-influxdb-password"
      }
    }
    ```

4. Build and run the application:

    ```bash
    dotnet build
    dotnet run
    ```

## Contributing

Contributions are welcome! Please feel free to open issues or submit pull requests for any improvements, bug fixes, or new features.

## License

This project is licensed under the [MIT License](LICENSE).

---

Feel free to reach out with any questions, suggestions, or feedback!
