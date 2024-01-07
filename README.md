![GitHub release (latest by date)](https://img.shields.io/github/v/release/suzuryg/local-switch-generator?label=release)

# LocalSwitchGenerator

This tool allows you to easily create local switch gimmicks in world production using the [Cluster Creator Kit](https://github.com/ClusterVR/ClusterCreatorKit).

- You can switch objects ON/OFF for each player
- Timeline method is used, so it can be applied to any object.
  - When used on Item-type objects, behavior may change from other players

## Requirements

- Unity 2021.3 / Cluster Creator Kit v1.17.0

## Installation

### from disk

1. Download and unzip the latest [release](https://github.com/suzuryg/local-switch-generator/releases)
1. Open the "Packages" window from the Unity menu Window > Package Manager
1. From the "+" button at the top left of the window, select "Add package from disk" and open the `package.json` in the unzipped folder

### [scoped registries](https://docs.unity3d.com/Manual/upm-scoped.html)

1. Open Packages/manifest.json and edit as follows:

```Packages/manifest.json
{
  "scopedRegistries": [
    {
      "name": "suzuryg",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.suzuryg" ]
    }
  ],
  "dependencies": {
    "jp.suzuryg.local-switch-generator": "1.0.0",
    ...
```

## How to Use

### Generate Local Switch

1. Open the tool window from Unity menu Tools > LocalSwitchGenerator
1. Set the object you want to switch ON/OFF in "Target Object" field
1. Set the object you want to use as the ON/OFF button in "Button" field
1. Press the "Generate" button to create the local switch gimmick
    - Item component and InteractItemTrigger component will be added to the button
    - A gimmick object named "Switch_XXX" (XXX is the name of the target object) will be created

### Delete Local Switch

1. Remove the Item component and InteractItemTrigger component from the button
1. Delete the "Switch_XXX" object

## References

[Creating Timeline-Style Local Gimmicks in Cluster](https://note.com/what_wat_/n/n722d6183ebee)

---

# LocalSwitchGenerator

[Cluster Creator Kit](https://github.com/ClusterVR/ClusterCreatorKit) を使用したワールド制作で、ローカルスイッチのギミックを簡単に作れるツールです。  

- プレイヤーごとにオブジェクトの ON / OFF を切り替えることができます
- Timeline方式なので、すべてのオブジェクトに対して使用できます
  - Item系のオブジェクトに使用した際は他プレイヤー視点での挙動が変化する場合があります

## 要件

- Unity 2021.3 / Cluster Creator Kit v1.17.0

## インストール

### from disk

1. 最新の [release](https://github.com/suzuryg/local-switch-generator/releases) からダウンロード、展開します
1. Unity メニュー Window > Package Manager から "Packages" ウィンドウを開きます
1. ウィンドウ内左上の "+" ボタンから "Add package from disk" を選択、展開したフォルダ内の `package.json` を開きます

### [scoped registries](https://docs.unity3d.com/Manual/upm-scoped.html)

1. Packages/manifest.json を開き、以下のように編集します

```Packages/manifest.json
{
  "scopedRegistries": [
    {
      "name": "suzuryg",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.suzuryg" ]
    }
  ],
  "dependencies": {
    "jp.suzuryg.local-switch-generator": "1.0.0",
    ...
```

## 使い方

### ローカルスイッチの生成

1. Unity メニュー  Tools > LocalSwitchGenerator から 本ツールのウィンドウ を開きます
1. 「対象オブジェクト」に ON / OFF を切り替えたいオブジェクトをセットします
1. 「ボタン」に ON / OFF を切り替えるボタンとするオブジェクトをセットします
1. 「生成」ボタンを押すと、ローカルスイッチのギミックが生成されます
    - ボタンにItemコンポーネントとInteractItemTriggerコンポーネントが追加されます
    - 「Switch_XXX」（XXXは対象オブジェクトの名前）という名前のギミック用オブジェクトが生成されます

### ローカルスイッチの削除

1. ボタンからItemコンポーネントとInteractItemTriggerコンポーネントを削除します
1. 「Switch_XXX」オブジェクトを削除します

## 参考文献

[clusterで作るTimeline式ローカルギミック](https://note.com/what_wat_/n/n722d6183ebee)
