# StudentReportChecker
### Еськова Ульяна Дмитриевна, БПИ236

## Архитектура микросервисов

Система построена по принципу микросервисной архитектуры и включает 2 основных сервиса + API:

### ApiGateway
- Принимает входящие HTTP-запросы
- Делает переадресацию к микросервисам
- Обрабатывает ошибки, в случае если сервисы недоступны 

### FileStoringService
- Принимает .txt файл
- Считает хеш файла, сохраняет, если ранее файл не был загружен
- Хранит путь к файлу, его имя и хеш
- Возвращает содержимое файла по ID

### FileAnalysisService
- Получает `fileId` от API Gateway
- Запрашивает содержимое файла у FileStoringService
- Считает:
  - количество абзацев
  - количество слов
  - количество символов
- Сохраняет результаты в собственной БД


## Описание маршрутов

### `POST /gateway/upload`
Загрузка .txt файла

**Request (multipart/form-data):**
- `file`: файл `.txt`

**Response:**
```json
{
  "fileId": "guid"
}
```

### `GET /gateway/analyze/{fileId}`
Для подсчета статистики содержимого файла

**Response:**
```json
{
  "id": "guid",
  "paragraphs": int,
  "words": int,
  "characters": int
}
```

---

### `GET /gateway/content/{fileId}`
Возвращает содержимое файла

**Response:**
```text
<текст из файла>
```

---

## Обработка ошибок

Если какой-либо сервис не отвечает, `ApiGateway` возвращает:

- `503 Service Unavailable` — если микросервис недоступен
- `500 Internal Server Error` — при других сбоях


## Запуск

```bash
docker-compose up --build
```

Swagger UI:
```
http://localhost:8080/swagger
```
