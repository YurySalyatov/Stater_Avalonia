# Changelog

## [Unreleased]

### Добавления
- Добавлена возможность запуска автомата
- Добавлено окно справа для контрукторов рекдактирования состояний, переходов..
- Добавлено окно слева для перехода между конечными автоматами

### Изменения
- Изменена версия C#: C# Framework 4 -> C# net 8
- Изменена директория с решением проекта, теперь основное решения находитсяв /src/Stater
- Изменен фреймворк для интерфейса Windows Forms -> Avalonia
- Изменена структура приложения, на данный момент приложение использует MVVM архитектуру
- Изменена система UNDO-REDO, раньше каждая команда требовала отдельного написания для класса, сейчас сохраняется каждое изменение StateMachine, что не требует функциональности для REDO
- Изменена система плагинов, на данный момент плагин не является отдельным решением