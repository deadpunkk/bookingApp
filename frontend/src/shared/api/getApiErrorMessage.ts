import { ApiError } from './apiClient';

const statusMessages: Record<number, string> = {
  400: 'Некорректные данные.',
  401: 'Сессия истекла или токен недействителен. Войдите снова.',
  403: 'Недостаточно прав для выполнения действия.',
  404: 'Ресурс не найден.',
  409: 'Конфликт бронирования. Выбранное время уже занято.',
  500: 'Ошибка сервера. Проверьте backend logs.',
};

const genericHttpMessages = new Set([
  'bad request',
  'unauthorized',
  'forbidden',
  'not found',
  'conflict',
  'internal server error',
]);

export function getApiErrorMessage(
  error: unknown,
  fallback = 'Не удалось выполнить запрос.',
): string {
  if (error instanceof ApiError) {
    const message = error.message.trim();
    const isGenericMessage = !message
      || message === `HTTP error ${error.status}`
      || genericHttpMessages.has(message.toLowerCase());

    if (!isGenericMessage) {
      return message;
    }

    return statusMessages[error.status] ?? (message || fallback);
  }

  if (error instanceof TypeError && error.message.toLowerCase().includes('fetch')) {
    return 'Не удалось связаться с backend. Проверьте, что API запущен.';
  }

  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return fallback;
}
