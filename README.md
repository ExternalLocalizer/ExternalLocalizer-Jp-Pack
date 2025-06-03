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
大型Modを含む**200以上**のModに対応しています。
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

> [!WARNING]
> 日本語対応フォントを含まないため、文字化けが発生する可能性があります。
> 以下のいずれかを併用してください。
> - 日本語対応フォントに置き換えるMod (例: [Kanamoji Fonts][steam-kanamoji])
> - 日本語対応フォントを含むリソースパック (例: [TrJpMod[EN] (日本語化パック)][steam-trjp])

## 注意事項
- 本Modは手動更新のため、[TMLHonyakuリポジトリ][gh-honyaku]と比べてやや更新が遅れる可能性があります。
- 本Modは[TMLHonyakuリポジトリ][gh-honyaku]のすべての翻訳用ファイルをロードします。MOD毎に個別に導入したい場合、リポジトリから必要なファイルをダウンロードし、[External Localizer][steam-el]に直接読み込ませてください。

## 権利表記
本Modが格納する翻訳ファイルには[synctam様][blog:synctam]による翻訳データをhjson形式に変換したものが含まれています。
したがって、テラリア本体の翻訳部分は「[CC BY-NC-SA 4.0（表示 - 非営利 - 継承 4.0 国際）][hp:CC]」を継承しています。

詳しい情報やその他のライセンスに関しては[TMLHonyakuリポジトリ][gh-honyaku]を参照してください。

## 免責事項

本プロジェクトは非公式の日本語翻訳であり、元のMOD制作者およびtModLoader公式とは一切関係ありません。
また翻訳の正確性や動作は保証されておらず、利用によって生じた不具合や損害について、当プロジェクトは責任を負いません。

分からないことがあれば、[Discord][discord-invite]もしくはIssueにてお問い合わせください。

[steam-el]:https://steamcommunity.com/sharedfiles/filedetails/?id=2986383249
[steam-eljp]:https://steamcommunity.com/sharedfiles/filedetails/?id=3401890281
[steam-kanamoji]:https://steamcommunity.com/sharedfiles/filedetails/?id=2461784420
[steam-trjp]:https://steamcommunity.com/sharedfiles/filedetails/?id=2865024243
[gh-honyaku]:https://github.com/ExternalLocalizer/TMLHonyaku
[gh-honyaku-csv]:https://github.com/ExternalLocalizer/TMLHonyaku/blob/main/TranslatedMods.csv
[discord-invite]:https://discord.gg/ch2DVxf2jY
[blog:synctam]: https://synctam.blogspot.com/2017/05/terraria_20.html
[hp:CC]: https://creativecommons.org/licenses/by-nc-sa/4.0/deed.ja
