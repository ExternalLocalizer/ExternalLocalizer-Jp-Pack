# # 更新用スクリプト

# Gitリポジトリ内に変更済みのファイルがないか確認
$gitStatus = git status --porcelain

# 変更済みのファイルが存在する場合は中断
if ($gitStatus) {
    Write-Host "Error: アップデートを行う前に変更済みのファイルをコミットしてください。"
    exit 1
}

# # TML Honyakuを更新
# git submodule update --remote

# ファイルパスを指定
$filePath = ".\build.txt"

# ファイル内容を読み込み
$content = Get-Content $filePath -Raw

# 現在の日付を取得 (yyyy.MM.dd形式)
$date = (Get-Date).ToString("yyyy.MM.dd")

# versionフィールドの正規表現パターンを定義
$versionPattern = 'version\s*=\s*([0-9]{4}\.[0-9]{2}\.[0-9]{2})\.(\d+)'

# 正規表現でバージョン番号を見つけて置換
$content = [regex]::replace($content, $versionPattern, {
        param($match)  # 正規表現の一致を受け取る
        # キャプチャした日付とインクリメント部分を変数に格納
        $oldDate = $match.Groups[1].Value
        $increment = [int]$match.Groups[2].Value
        
        # 日付が変更された場合はインクリメントをリセット、そうでなければインクリメントを増加
        if ($oldDate -ne $date) {
            $newVersion = "$date.0"
        }
        else {
            $newVersion = "$date." + ($increment + 1)
        }
        
        # 新しいバージョン番号を返す
        return "version = $newVersion"
    });

Write-Host "New content:"
Write-Host $content

# 新しい内容をファイルに書き戻し
Set-Content -Path $filePath -Value $content -NoNewline

# 変更をコミット
git add $filePath
git commit -m "Update version to $date"
git push
