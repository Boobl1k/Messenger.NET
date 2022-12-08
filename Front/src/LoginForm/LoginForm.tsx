import React, {useState} from "react";
import {Link} from "react-router-dom";

export default function LoginForm() {
    const [name, setName] = useState('');
    
    const onSetUserName = (event: React.ChangeEvent<HTMLInputElement>) => setName(event.target.value);

    return <>
        <input type={"text"} onChange={onSetUserName} placeholder={"Your name"}/>
        <Link to={`wait/admin/${name}`}>Login as admin</Link>
        <Link to={`wait/user/${name}`}>Login as user</Link>
    </>
}