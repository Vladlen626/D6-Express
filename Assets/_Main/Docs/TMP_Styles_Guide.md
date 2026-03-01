# TMP Styles Guide

This project uses TextMeshPro style tags to highlight fragments inside localized strings.

## 1) Create a Style Sheet

1. `Assets -> Create -> TextMeshPro -> Style Sheet`
2. Name it `UIStyles`.

## 2) Add a Style

In the `UIStyles` asset, add a style entry:

- `Style Name`: `ItemName`
- `Opening Tag`: `<color=#FFD54F><b>`
- `Closing Tag`: `</b></color>`

You can combine tags as needed (examples):
- `<i>` italics
- `<u>` underline
- `<size=120%>` size
- `<alpha=#AA>` alpha

## 3) Connect the Style Sheet

Choose one:

- Global: `Project Settings -> TextMeshPro -> Default Style Sheet`
- Per-text: `TextMeshProUGUI` component -> `Style Sheet` field

## 4) Use in Localization Strings

Example:

```
Bought <style="ItemName">"{0}"</style>
```

Notes:
- `Rich Text` must be enabled on the TMP component.
- `style="ItemName"` must match the exact `Style Name`.
