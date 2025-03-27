# Batalha Naval

### Estrutura de Pastas

```
NavalBattle/
├── Domain/
│   ├── Models/
│   │   ├── Ship.cs
│   │   ├── BattleField.cs
│   │   ├── Position.cs
│   │   └── Message.cs (já existe)
│   ├── Enums/
│   │   ├── EventType.cs
│   │   ├── ShipOrientation.cs
│   │   └── AttackResult.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IMessageService.cs
│   │   │   ├── ICryptoService.cs
│   │   │   └── IBattleService.cs
│   │   └── Implementations/
│   │       ├── MessageService.cs
│   │       ├── CryptoService.cs
│   │       └── BattleService.cs
│   └── Exceptions/
│       └── BattleException.cs
├── Infrastructure/
│   ├── Azure/
│   │   └── ServiceBus/
│   │       ├── ServiceBusClient.cs
│   │       └── ServiceBusConfig.cs
│   └── Crypto/
│       └── CryptoProvider.cs
└── Application/
    └── Services/
        └── BattleCoordinator.cs
```
