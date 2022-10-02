import React, {useEffect, useState} from 'react';
import './App.css';
import {List, Button, ListItem, ListItemText, Box, Divider, TextField} from "@mui/material";
import IMessage from "./entities/IMessage";
import {Chat} from "./Chat/Chat";

// const BASE_URL = 'https://localhost:5002/';
// const SEND_MESSAGE = 'SendMessageMt';
// const MESSAGES = 'messages';
//
// const fetchFromBack = (url: string) => fetch(BASE_URL + url).then(res => res.json());
// const postToBack = (url: string, data: any) => fetch(
//     BASE_URL + url,
//     {
//         method: 'POST',
//         body: JSON.stringify(data),
//         headers: {'content-type': 'application/json'}
//     });

function App() {
    return (
        <div className="App">
            <div style={{ margin: '0 30%' }}>
                <Chat />
            </div>
        </div>
    );
}

export default App;
