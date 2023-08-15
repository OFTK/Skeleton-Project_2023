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
            showNotification(message);
        });

        connection.start()
            .catch(console.error);

        return () => {
            connection.stop().catch(console.error);
        };
    }, []);

    const showNotification = (message) => {
        console.log('message:', message);
        if (!message || !message.alert_list || message.alert_list.length === 0) {
            return;
        }
        const alertInfo = message.alert_list[0];
        if (alertInfo) {
            const babyname = alertInfo.babyname || 'no baby name';
            const alertreason = alertInfo.alertreason || 'no alert reason, signalR Notifications';
            const formattedMessage = `alert: babyname: ${babyname}, alert reason: ${alertreason}`;
            toast.info(formattedMessage);
        }
    };

    return (
        <div>
            <h2>Received Messages:</h2>
            <ul style={{ paddingLeft: 0, listStyle: 'none' }}>
                {messages.map((message, index) => (
                    <li key={index} style={{ textAlign: 'left', marginLeft: '2rem', marginBottom: '0.5rem' }}>
                        alert- babyname: {message.alert_list[0]?.babyname || 'no baby name'}, 
                        alert reason: {message.alert_list[0]?.alertreason || 'no alert reason, signalR Notifications'}
                    </li>
                ))}
            </ul>
            <ToastContainer />
        </div>
    );
};

export default SignalRNotifications;
