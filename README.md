# Word Frequency Counter - Azure Function

This Azure Function is designed to count the frequency of words in text files uploaded via HTTP POST requests.
It processes each file concurrently, counts the occurrences of each word, and returns the word counts in descending order.

## Application Overview

- **Function Name**: \"WordCount\"
- **Trigger**: HTTP Trigger (POST)
- **Tech Stack**: C# 12.0 /.NET 8.0, Azure Functions
- **Processing**: Incoming files are processed concurrently between threads.

## Usage

1. **Upload Files**: Send an HTTP POST request with text files attached as form data with any name.

2. **Response**: Receives a JSON response with word counts in descending order.

*Response Example*:
```json
{
    "do": 2,
    "so": 1,
    "the": 1,
    "well": 1,
    "you": 1,
    "things": 1
}
```

## Deployment
The application is deployed in Azure and can be accessed remotely.

**Remote API Endpoint URL**: https://apwordfrequencycounter.azurewebsites.net/api/WordCount


### API Request:

- Use the [Postman collection to send Requests](3Shape%20Word%20Counting.postman_collection.json).
- Attach any test text file as form data. Or use [this one](TestFiles/do%20so%20well.txt).
