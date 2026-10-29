import React from 'react';
import '../styles/ConnectionStatus.css';

function ConnectionStatus({ isConnected, hubUrl }) {
  return (
    <div className={`connection-status ${isConnected ? 'connected' : 'disconnected'}`}>
      <span className="status-indicator"></span>
      <div>
        <p className="status-text">
          {isConnected ? '✓ Подключено' : '✗ Отключено'}
        </p>
        <p className="hub-url">{hubUrl}</p>
      </div>
    </div>
  );
}

export default ConnectionStatus;
