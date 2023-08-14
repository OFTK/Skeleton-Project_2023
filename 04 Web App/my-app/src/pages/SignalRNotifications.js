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
        const alertInfo = message.alert_list[0];
        const formattedMessage = `alert: babyid: ${alertInfo.babyid}, babyname: ${alertInfo.babyname}, alert reason: ${alertInfo.alertreason}`;
        toast.info(formattedMessage);
    };

    return (
        <div>
            <h2>Received Messages:</h2>
            <ul>
                {messages.map((message, index) => (
                    <li key={index}>
                        alert: babyid: {message.alert_list[0].babyid}, babyname: {message.alert_list[0].babyname}, alert reason: {message.alert_list[0].alertreason}
                    </li>
                ))}
            </ul>
           
            <ToastContainer /> {/* Container for toast notifications */}
        </div>
    );
};

export default SignalRNotifications;
