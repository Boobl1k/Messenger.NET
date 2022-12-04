import React, {useState} from "react";
import {Link} from "react-router-dom";

export default function LoginForm() {
    const [userName, setUserName] = useState('');
    const [adminName, setAdminName] = useState('');
    
    const onSetUserName = (event: React.ChangeEvent<HTMLInputElement>) => setUserName(event.target.value);
    const onSetAdminName = (event: React.ChangeEvent<HTMLInputElement>) => setAdminName(event.target.value);

    return <>
        <input type={"text"} onChange={onSetUserName} placeholder={"user name"}/>
        <br/>
        <input type={"text"} onChange={onSetAdminName} placeholder={"admin name"}/>
        <Link to={`chat/admin/${userName}/${adminName}`}>Login as admin</Link>
        <Link to={`chat/user/${userName}/${adminName}`}>Login as user</Link>
    </>
}