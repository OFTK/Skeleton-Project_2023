import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const SignalRNotifications = () => {
    const [messages, setMessages] = useState([]);

    useEffect(() => {
        const apiBaseUrl = "https://ilovemybaby.azurewebsites.net";
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(apiBaseUrl + '/api')
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on('babyalert', (message) => {
            setMessages(prevMessages => [...prevMessages, message]);
            toast.info(JSON.stringify(message).substring(1, JSON.stringify(message).length - 1));
        });

        connection.start()
            .catch(console.error);

        return () => {
            connection.stop().catch(console.error);
        };
    }, []);

    return (
        <div>
            <h2>NEW BABY ALERT:</h2>
            <ul>
                {messages.map((message, index) => (
                    <li key={index}>{JSON.stringify(message)}</li>
                ))}
            </ul>
            <ToastContainer /> {/* Container for toast notifications */}
        </div>
    );
};

export default SignalRNotifications;
