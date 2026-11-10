# QA Test Cases – OOP10FEXLab

Tester: ____________________  Date: ____________

Run with **Open Both (demo)** from the launcher. Mark each test Pass/Fail.

## A. Kiosk (MJ)

| ID | Steps | Expected result | P/F |
|---|---|---|---|
| K-01 | Launch kiosk | Welcome screen shows DINE IN and TAKE OUT | |
| K-02 | Tap DINE IN | Menu opens with "DINE IN" in the top-right; all meals from meals.txt are shown | |
| K-03 | Tap each category on the left | Only meals of that category are shown, and the active category turns yellow | |
| K-04 | Tap a meal card twice | The cart shows that meal with Qty 2 and the correct subtotal and total | |
| K-05 | Select cart row, press + / − | Qty changes; at 0 the row disappears | |
| K-06 | Press + 25 times on one item | Qty stops at 20 and a "Maximum of 20" message appears | |
| K-07 | Press + without selecting a row | The message "Select an item in your order first" appears | |
| K-08 | Empty cart | The "Review & Pay" button is disabled | |
| K-09 | Add items, press Cancel → No | The cart is kept | |
| K-10 | Add items, press Cancel → Yes | The kiosk goes back to the Welcome screen with an empty cart | |
| K-11 | Review & Pay | The summary lists every item with qty and subtotal, and the total matches the cart | |
| K-12 | Back to Menu | The cart is still intact | |
| K-13 | Choose "Card" | The order number screen appears (1001 for the first order) | |
| K-14 | Open `bin\…\Data\transactions.txt` | A new line exists with the number, date, type, payment, Preparing, total and items | |
| K-15 | Wait 10 seconds on the order number screen | The kiosk auto-returns to Welcome | |
| K-16 | Add a meal line to meals.txt and restart | The new meal appears, with no code change needed | |
| K-17 | Add a broken line (`X1|Bad|Cat|abc`) and restart | The kiosk still opens and the broken line is skipped | |

## B. Queue Monitor (Raf)

| ID | Steps | Expected result | P/F |
|---|---|---|---|
| Q-01 | Open the monitor with no orders | The PREPARING and NOW SERVING columns are empty and the clock is ticking | |
| Q-02 | Place an order on the kiosk | Its number appears under PREPARING within 2 s | |
| Q-03 | Place 3 more orders | All numbers are listed in ascending order | |
| Q-04 | Select the number and click "Order Ready >" | The number moves to NOW SERVING (big, red), the column flashes and a sound plays | |
| Q-05 | Select the number under Now Serving and click "Claimed" | The number disappears from the monitor | |
| Q-06 | Click a button with no orders | The message "No order selected" appears | |
| Q-07 | Press F2 | The crew bar hides (customer view); press again to show it | |
| Q-08 | Press F11 | Full screen toggles | |
| Q-09 | Close and reopen the monitor | Statuses are kept (read from transactions.txt) | |
| Q-10 | Add a garbage line to transactions.txt | The monitor keeps working and ignores the line | |

## C. Integration

| ID | Steps | Expected result | P/F |
|---|---|---|---|
| I-01 | Run kiosk + monitor together, place 5 orders quickly | No crash, no duplicate numbers | |
| I-02 | Mark orders ready while ordering on the kiosk | Both screens keep working | |
| I-03 | Restart the app, place an order | Numbering continues from the last order (it does not reset to 1001) | |

## Automated logic check (done by developer)

The core logic (cart rules, file read/write, numbering, status flow, bad-data handling) was verified
with 19 automated checks, and all of them passed.
