# Batalha Naval

## Descrição

O projeto "Batalha Naval" é uma implementação de um jogo de batalha naval, onde os jogadores controlam navios e tentam atacar os navios inimigos em um tabuleiro. O jogo utiliza criptografia para proteger as mensagens trocadas entre os navios, garantindo que as informações sobre as posições dos navios inimigos sejam mantidas em segredo.

## Estrutura de Pastas

```
NavalBattle/
├── Core/
│ ├── Models/ # Contém as classes de modelo, como Ship, Position e Message.
│ │ ├── MessageContent/ # Contém as classes de conteúdo das mensagens, como ResultadoAtaqueContent e RegistroNavioContent.
│ ├── Enums/ # Contém enums utilizados no projeto, como ShipOrientation e EventType.
| ├── Helpers/ # Contém classes auxiliares, como JsonHelper.
│ └── Exceptions/ # Contém exceções personalizadas para o projeto.
├── Application/
│ ├── Services/ # Contém a lógica de negócios e serviços do jogo.
│ │ └── Implementations/ # Implementações dos serviços, como MessageService e CryptoService.
│ └── Interfaces/ # Contém as interfaces para os serviços.
└── Infrastructure/
  └── Config/ # Contém a configuração do aplicativo, como AppSettings.
```

## Funcionalidades

- **Registro de Navios**: Os navios são registrados no início do jogo com uma posição central e orientação (horizontal ou vertical).
- **Ataques**: Os jogadores podem atacar posições no tabuleiro, e o resultado do ataque é retornado, informando se houve acerto ou erro, juntamente com a distância do ataque em relação ao navio inimigo.
- **Criptografia**: As mensagens trocadas entre os navios são criptografadas para proteger as informações sobre as posições dos navios inimigos.
- **Estratégia de Ataque**: O sistema utiliza uma estratégia de ataque que prioriza posições conhecidas e descarta áreas que não são mais relevantes com base nos resultados dos ataques anteriores.

## Requisitos

- **Azure Service Bus**: É necessário ter um tópico configurado no Azure Service Bus para que a comunicação entre os navios funcione corretamente.
- **Controlador**: O projeto funciona como um navio e requer um controlador para gerenciar as partidas.

## Tecnologias Utilizadas

- **C#**: Linguagem de programação utilizada para desenvolver o projeto.
- **.NET**: Framework utilizado para construir a aplicação.
- **Azure Service Bus**: Utilizado para a comunicação entre os navios.
- **Criptografia AES**: Implementada para proteger as mensagens trocadas.

## Configuração

Para configurar o projeto, você precisará ajustar o arquivo `appsettings.Development.json` com suas credenciais do Azure Service Bus e a chave criptográfica:

```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    "TopicName": "your-topic-name",
    "SubscriptionName": "your-subscription-name"
  },
  "Ship": {
    "Name": "your-ship-name",
    "CryptoKey": "your-crypto-key"
  }
}
```

## Como Executar

1. Clone o repositório.
2. Abra o projeto no Visual Studio ou em outro IDE compatível com .NET.
3. Ajuste as configurações no arquivo `appsettings.Development.json`.
4. Execute o projeto.

## Contribuidores
- [Aira Arima](https://github.com/airaarima)
- [Karina Bertolazzo](https://github.com/karinabertolazzo)
- [Patricia Ferrer](https://github.com/patsferrer)
