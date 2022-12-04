import React from 'react';
import './App.css';
import Chat from "./Chat/Chat";
import {BrowserRouter, Route, Routes} from "react-router-dom";
import {LoginForm} from './LoginForm';

function App() {
    return (
        <div
            className="flex flex-col items-center justify-center w-screen min-h-screen bg-gray-100 text-gray-800 p-10">
            <BrowserRouter>
                <Routes>
                    <Route path={'/'} element={<LoginForm/>}/>
                    <Route path={'/chat/admin/:userName/:adminName'} element={<Chat isAdmin={true}/>}/>
                    <Route path={'/chat/user/:userName/:adminName'} element={<Chat isAdmin={false}/>}/>
                </Routes>
            </BrowserRouter>
        </div>
    );
}

export default App;
