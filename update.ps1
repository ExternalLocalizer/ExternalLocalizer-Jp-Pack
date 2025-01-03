# 更新用スクリプト

# TML Honyakuを更新
git submodule update --remote

# ファイルパスを指定
$filePath = ".\build.txt"

# ファイル内容を読み込み
$content = Get-Content $filePath

# 現在の日付を取得 (yyyy.MM.dd形式)
$date = (Get-Date).ToString("yyyy.MM.dd")

# 正規表現でバージョン番号を見つけて置換
$content = $content -replace 'version\s*=\s*([0-9]{4}\.[0-9]{2}\.[0-9]{2})\.(\d+)', {
    # キャプチャした日付とインクリメント部分を変数に格納
    $oldDate = $matches[1]
    $increment = [int]$matches[2]
    
    # 日付が変更された場合はインクリメントをリセット、そうでなければインクリメントを増加
    if ($oldDate -ne $date) {
        $newVersion = "$date.0"
    }
    else {
        $newVersion = "$date." + ($increment + 1)
    }
    
    # 新しいバージョン番号を返す
    return "version = $newVersion"
}

# 新しい内容をファイルに書き戻し
Set-Content -Path $filePath -Value $content
