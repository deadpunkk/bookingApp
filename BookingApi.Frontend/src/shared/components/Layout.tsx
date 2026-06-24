import {Link, Outlet} from 'react-router';

export function Layout(){
    return (
        <div className='app'>
            <header className='app-header'>
                <div className='app-logo'>BookingApi</div>

                <nav className="app-nav">
                    <Link to="/bookings">Все бронирования</Link>
                    <Link to="/my-bookings">Мои бронирования</Link>
                    <Link to="/bookings/create">Создать бронь</Link>
                    <Link to="/login">Войти</Link>
                </nav>
            </header>

            <main className="app-main">
                <Outlet />
            </main>
        </div>
    );
}