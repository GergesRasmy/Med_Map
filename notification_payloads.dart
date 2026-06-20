// Mirror of Med_Map.DTO.NotificationDTOs — keep in sync with NotificationPayloads.cs
// Hub endpoint: /hubs/notifications?access_token=<jwt>
// Package needed: signalr_netcore

class OrderPlacedPayload {
  final String orderId;
  final String customerId;
  final double totalAmount;
  final int itemCount;
  final String fulfillmentType; // "Delivery" | "Pickup"
  final DateTime createdAt;

  OrderPlacedPayload.fromJson(Map<String, dynamic> j)
      : orderId = j['orderId'],
        customerId = j['customerId'],
        totalAmount = (j['totalAmount'] as num).toDouble(),
        itemCount = j['itemCount'],
        fulfillmentType = j['fulfillmentType'],
        createdAt = DateTime.parse(j['createdAt']);
}

class OrderStatusChangedPayload {
  final String orderId;
  final String newStatus; // "Recorded" | "Packaged" | "OutForDelivery" | "ReadyForPickup" | "Delivered" | "Canceled"
  final String fulfillmentType;

  OrderStatusChangedPayload.fromJson(Map<String, dynamic> j)
      : orderId = j['orderId'],
        newStatus = j['newStatus'],
        fulfillmentType = j['fulfillmentType'];
}

class OrderCancelledPayload {
  final String orderId;
  final String customerId;

  OrderCancelledPayload.fromJson(Map<String, dynamic> j)
      : orderId = j['orderId'],
        customerId = j['customerId'];
}

class WalletDepositedPayload {
  final String walletId;
  final double amount;
  final String currency; // "EGP"
  final String orderId;

  WalletDepositedPayload.fromJson(Map<String, dynamic> j)
      : walletId = j['walletId'],
        amount = (j['amount'] as num).toDouble(),
        currency = j['currency'],
        orderId = j['orderId'];
}

class StockChange {
  final String medicineId;
  final String medicineName;
  final int newStockQuantity;

  StockChange.fromJson(Map<String, dynamic> j)
      : medicineId = j['medicineId'],
        medicineName = j['medicineName'],
        newStockQuantity = j['newStockQuantity'];
}

class InventoryStockChangedPayload {
  final List<StockChange> changes;

  InventoryStockChangedPayload.fromJson(Map<String, dynamic> j)
      : changes = (j['changes'] as List)
            .map((e) => StockChange.fromJson(e as Map<String, dynamic>))
            .toList();
}

class WithdrawalCompletedPayload {
  final String transactionId;
  final String walletId;

  WithdrawalCompletedPayload.fromJson(Map<String, dynamic> j)
      : transactionId = j['transactionId'],
        walletId = j['walletId'];
}

class WithdrawalCancelledPayload {
  final String transactionId;
  final String walletId;
  final double refundedAmount;
  final String currency;

  WithdrawalCancelledPayload.fromJson(Map<String, dynamic> j)
      : transactionId = j['transactionId'],
        walletId = j['walletId'],
        refundedAmount = (j['refundedAmount'] as num).toDouble(),
        currency = j['currency'];
}

// ---------------------------------------------------------------------------
// Example connection setup (place in a singleton service / provider):
//
// import 'package:signalr_netcore/signalr_client.dart';
//
// final hub = HubConnectionBuilder()
//     .withUrl('$baseUrl/hubs/notifications?access_token=$jwt')
//     .withAutomaticReconnect()
//     .build();
//
// hub.on('OrderPlaced', (args) {
//   final p = OrderPlacedPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh pharmacy order list
// });
// hub.on('OrderStatusChanged', (args) {
//   final p = OrderStatusChangedPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh customer order detail
// });
// hub.on('OrderCancelled', (args) {
//   final p = OrderCancelledPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh pharmacy order list
// });
// hub.on('WalletDeposited', (args) {
//   final p = WalletDepositedPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh pharmacy wallet
// });
// hub.on('InventoryStockChanged', (args) {
//   final p = InventoryStockChangedPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh pharmacy inventory
// });
// hub.on('WithdrawalCompleted', (args) {
//   final p = WithdrawalCompletedPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh pharmacy wallet transactions
// });
// hub.on('WithdrawalCancelled', (args) {
//   final p = WithdrawalCancelledPayload.fromJson(args![0] as Map<String, dynamic>);
//   // refresh pharmacy wallet (balance was refunded) and transactions
// });
//
// await hub.start();
