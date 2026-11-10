# OOP10FEXLab – Kiosk & Queue Monitor

Self-order kiosk and claim/queue monitor, like the ones in fast-food chains (Jollibee, McDo, KFC).
C# Windows Forms (.NET 8). All screens are built in code, so there are no Designer files to break.

## How it works

```
 [ KIOSK ]                         [ QUEUE MONITOR ]
 meals.txt ──► menu                transactions.txt ──► PREPARING | NOW SERVING
 customer orders ──► transactions.txt ──► (auto refresh every 2 s)
                                    crew: "Order Ready >"  →  moves to NOW SERVING
                                    crew: "Claimed"        →  removed from screen
```

* **Package meals** are loaded from `Data\meals.txt` (the notepad database) or from a real database.
* **Every order is saved** to the transactions record `Data\transactions.txt`, which sits next to the .exe
  (`OOP10FEXLab\bin\Debug\net8.0-windows\Data\`).
* Order numbers start at **1001** and continue from the last saved order.

## Who did what

| Member | Part | Files |
|---|---|---|
| MJ | **Kiosk** | `Kiosk/KioskForm.cs`, `Kiosk/Cart.cs`, `Kiosk/MealCard.cs` |
| Raf | **Queue Monitor** | `QueueMonitor/QueueMonitorForm.cs`, `QueueMonitor/QueueService.cs` |
| Member 3 | **QA / Testing** | `TEST_CASES.md`, `names.txt` |
| Shared | Models + data layer | `Core/…`, `Program.cs`, `LauncherForm.cs`, `Data/meals.txt` |

## How to run

1. Open `OOP10FEXLab.sln` in Visual Studio 2022.
2. Press **F5**. The launcher appears.
3. Click **Open Both (demo)**: the kiosk is on the left and the monitor on the right.
4. Order on the kiosk and pay. The number shows on the monitor under **PREPARING** within 2 seconds.
5. On the monitor, pick the number and click **Order Ready >**, then **Claimed**.

Monitor shortcuts: **F2** hides/shows the crew bar (customer view) and **F11** toggles full screen.

## Switching to the database (when Prof. gives the connection code)

Open `Core/AppServices.cs` and replace the `Meals = …` line:

```csharp
Meals = new DatabaseMealRepository(
    () => new MySqlConnection("<paste connection string here>"),
    "SELECT code, name, category, price, inclusions FROM meals");
```

Nothing else changes, because the kiosk only depends on the `IMealRepository` interface.
(For MySQL, install the **MySql.Data** NuGet package and add `using MySql.Data.MySqlClient;`.)

## OOP concepts used

* **Encapsulation**: `Cart` keeps its list private and enforces the 1–20 quantity rule. `Meal` validates in its constructor.
* **Abstraction / Interfaces**: `IMealRepository`, `ITransactionRepository`.
* **Polymorphism**: `TextFileMealRepository` and `DatabaseMealRepository` are interchangeable.
* **Inheritance**: `MealCard : Panel`, and every form inherits `Form`.
* **Events**: `Cart.Changed` and `MealCard.MealSelected`.
* **Enums**: `OrderStatus` (Preparing → NowServing → Claimed) and `OrderType` (DineIn / TakeOut).
