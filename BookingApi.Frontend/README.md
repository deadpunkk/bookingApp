# BookingApi Frontend

React-клиент для BookingApi. Проект использует React, TypeScript, Vite, React Router,
Context API и обычный CSS. Redux и UI-фреймворки не используются.

## Запуск

Требуется актуальная LTS-версия Node.js и запущенный BookingApi.

```bash
npm install
cp .env.example .env.local
npm run dev
```

Dev server Vite по умолчанию доступен на `http://localhost:5173`.

## Переменные окружения

```env
VITE_API_BASE_URL=http://localhost:8080
```

Если переменная не задана, frontend использует `http://localhost:8080`.
Файл `.env.local` игнорируется Git, а `.env.example` можно хранить в репозитории.

## Страницы

- `/login` — вход пользователя.
- `/bookings` — все бронирования с серверной пагинацией.
- `/my-bookings` — бронирования текущего пользователя.
- `/bookings/create` — создание брони для Admin и Moderator.
- `/bookings/:id/edit` — редактирование брони для Admin и Moderator.

Защищённые страницы перенаправляют гостя на `/login`, а после успешного входа
возвращают его на исходный URL. Viewer не получает доступ к маршрутам создания и
редактирования и не видит управляющие кнопки.

## Backend и авторизация

Вход выполняется через `POST /users/login`. Полученный `accessToken` сохраняется в
`localStorage`, а числовая роль (`0`, `1`, `2`) нормализуется в `Admin`, `Moderator`
или `Viewer`. Защищённые запросы отправляют заголовок
`Authorization: Bearer <accessToken>`.

Общий HTTP-клиент и обработка ошибок находятся в `src/shared/api`. Auth API и
состояние авторизации находятся в `src/features/auth`, а все запросы бронирований —
в `src/features/bookings/bookingsApi.ts`.

Frontend использует endpoints:

- `GET /bookings?page=1&pageSize=10`
- `GET /bookings/my`
- `GET /bookings/{id}`
- `POST /bookings`
- `PUT /bookings/{id}`
- `DELETE /bookings/{id}`

## Проверка

```bash
npm run lint
npm run build
```

Production build создаётся в каталоге `dist`.
