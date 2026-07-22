import { AuthProvider } from '../features/auth/AuthProvider';

import { AppRouter } from './Router';

export function App() {
  return (
    <AuthProvider>
      <AppRouter />
    </AuthProvider>
  );
}