# Procedural Horror Maze — Unity Integration Guide

## Структура файлов

```
ProceduralMaze/
├── RoomData.cs               — ScriptableObject с параметрами каждого типа комнаты
├── MazeCell.cs               — Данные ячейки сетки (не MonoBehaviour)
├── MazeGenerator.cs          — Основной генератор лабиринта
├── RoomController.cs         — Компонент на каждом префабе комнаты
├── PlayerHealthController.cs — Здоровье игрока (для тёмных комнат)
└── MazeDebugVisualizer.cs    — Gizmos-карта лабиринта в редакторе
```

---

## Быстрый старт

### 1. Создать Room Data ассеты

`Assets → Create → Horror Maze → Room Data`

Создайте как минимум:
- `StartRoom`  (roomType = Start)
- `EndRoom`    (roomType = End)
- 2–3 `NormalRoom_XX` (roomType = Normal, spawnWeight = 0.7)
- 1–2 `DarkRoom_XX`   (roomType = Dark,   spawnWeight = 0.3, maxDarkTime = 8, darkDamagePerSecond = 5)

### 2. Настроить префабы комнат

На каждый префаб добавьте компонент `RoomController`.
В инспекторе:
- `doorNorth/South/East/West` — ссылки на дочерние объекты-стены
- `roomLight` — источник света внутри комнаты
- `darkFogRenderer` — плоскость с туманным материалом (для тёмных комнат)

Коллайдер на корне префаба должен быть **IsTrigger = true**,
охватывать всю площадь комнаты.

### 3. Создать объект генератора

1. Создайте пустой GameObject `MazeGenerator`.
2. Добавьте компоненты `MazeGenerator` и `MazeDebugVisualizer`.
3. Заполните поля:
   - `startRoomData`, `endRoomData` — ваши ассеты
   - `normalRooms`, `darkRooms` — списки ассетов
   - `gridWidth` / `gridHeight` — размер сетки
   - `cellSize` — должен совпадать с реальным размером префаба комнаты
   - `mode` — Maze (полный лабиринт) или Corridor (длинный путь)

4. Нажмите ▶ или вызовите через ПКМ → **Generate Maze** в контекстном меню компонента.

### 4. Настроить игрока

Добавьте `PlayerHealthController` на объект игрока.
Убедитесь, что у игрока тег `Player` (`Edit → Project Settings → Tags`).

---

## Режимы генерации

| Параметр | Maze | Corridor |
|---|---|---|
| Заполнение сетки | Полное | Только основной путь + ответвления |
| Гарантированный путь | Да (самый длинный по DFS) | Да (`minPathLength`) |
| Ветвление | Встроено в DFS | `branchProbability` |
| Для чего | Классический лабиринт | Хоррор-коридор с тупиками |

---

## Темные комнаты — как работает механика

```
Игрок входит (OnTriggerEnter)
    └─► StartDarkTimer()
            ├─ 0..60% maxDarkTime  — тихо, таймер тикает
            ├─ 60%..100%           — warningAudio воспроизводится
            └─ > maxDarkTime       — каждый кадр: TakeDamage(dps * dt)

Игрок выходит (OnTriggerExit)
    └─► StopDarkTimer() — таймер сбрасывается, урон прекращается
```

Параметры в `RoomData`:
- `maxDarkTime` — сколько секунд игрок может оставаться без последствий
- `darkDamagePerSecond` — урон/сек после истечения времени

---

## Расширение

### Добавить новый тип комнаты
1. Добавьте значение в `enum RoomType` в `RoomData.cs`
2. Создайте новый `RoomData` ассет с этим типом
3. Добавьте поведение в `RoomController.ConfigureVisuals()` или через `onPlayerEnter` Event в инспекторе

### Подписаться на события генерации
```csharp
var gen = GetComponent<MazeGenerator>();
gen.OnMazeGenerated += (grid) => {
    // grid[x, z].RoomData, grid[x, z].Instance
    Debug.Log("Maze ready!");
};
gen.Generate();
```

### Перегенерация во время игры
```csharp
mazeGenerator.ClearMaze();   // удалит все инстансы
mazeGenerator.seed = 0;      // 0 = случайный
mazeGenerator.Generate();    // создаст новый
```

---

## Советы по производительности

- Включайте `generateAsync = true` и ставьте `framesPerRoom = 1` для плавной загрузки.
- При больших сетках (>15×15) рассмотрите Object Pooling вместо `Instantiate`.
- `MazeDebugVisualizer` работает только в Editor — в билде нет затрат.
