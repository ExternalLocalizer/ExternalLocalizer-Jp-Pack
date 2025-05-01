<img src="icon_workshop.png" height="75px" align=left alt="logo">
<H1>ExternalLocalizer-Jp-Pack </H1>

<div style="float:left;">
  <img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/ExternalLocalizer/ExternalLocalizer-Jp-Pack"/>
  <img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/ExternalLocalizer/ExternalLocalizer-Jp-Pack/update-localizations.yml"/>
  <a target="_blank" href="https://discord.gg/ch2DVxf2jY">
    <img alt="Discord Server" src="https://dcbadge.limes.pink/api/server/ch2DVxf2jY?style=flat"/>
  </a>
</div>


## 概要
Japanese Packには[TMLHonyaku][gh-honyaku]で公開されている日本語化ファイルが内蔵されています。
[External Localizer][steam-el]と併用することで、簡単にModを日本語化できます。

## 主な対応Mod
大型Modを含む**150以上**のModに対応しています。
- Calamity Mod
- Thorium Mod
- Fargo's Souls Mod
- Boss Cursor
- Boss Checklist
- Item Checklist
- Recipe Browser
など。
完全なリストは[TranslatedMods.csv][gh-honyaku-csv]を参照してください。

## 使い方
[External Localizer][steam-el]と[本Mod][steam-eljp]をサブスクライブし、ゲーム内のModリストから有効化してください。

その後ExternalLocalizerの設定画面で、以下の項目が正しく設定されていることを確認してください。
- `ローカライズを有効化`がオンになっていること
- `言語キー`が`ja-JP`に設定されていること

## 注意事項
- 本Modは手動更新のため、[TMLHonyakuリポジトリ][gh-honyaku]と比べてやや更新が遅れる可能性があります。
- 本Modは[TMLHonyakuリポジトリ][gh-honyaku]のすべての翻訳用ファイルをロードします。MOD毎に個別に導入したい場合、リポジトリから必要なファイルをダウンロードし、[External Localizer][steam-el]に直接読み込ませてください。

分からないことがあれば、[Discord][discord-invite]もしくはIssueにてお問い合わせください。

[steam-el]:https://steamcommunity.com/sharedfiles/filedetails/?id=2986383249
[steam-eljp]:https://steamcommunity.com/sharedfiles/filedetails/?id=3401890281
[gh-honyaku]:https://github.com/ExternalLocalizer/TMLHonyaku
[gh-honyaku-csv]:https://github.com/ExternalLocalizer/TMLHonyaku/blob/main/TranslatedMods.csv
[discord-invite]:https://discord.gg/ch2DVxf2jY
