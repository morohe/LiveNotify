# LiveNotify
動画配信サイトから配信通知を受け取るプログラムです。

# 動作環境
現在Windows 10 Pro 1709で確認しています。
ランタイムとして.NET Framework 4.6.2以上のインストールが必須です。
以下からダウンロードしてインストールしてください。

[https://www.microsoft.com/ja-jp/download/details.aspx?id=56115]([https://www.microsoft.com/ja-jp/download/details.aspx?id=56115)

# ダウンロード
下記のページからzipファイルをダウンロードしてください。

[https://github.com/morohe/LiveNotify/releases](https://github.com/morohe/LiveNotify/releases)

# インストール
ZIPファイルを解凍するだけです。

# 使い方
* LiveNotify.exeを起動すると右下のトレイに灰色の変なのが出るのでそれをダブルクリックすると配信一覧のウィンドウが出ます。
上は現在の配信一覧、下はお気に入りです。
* 検索の所になにか見たい配信のタイトルなりユーザーIDを入れてその横のドロップダウンで検索対象を選択、検索ボタンで引っかかるかどうか確認、お気に入りに追加で、配信が始まったら通知するようにできます。
* クリアボタンは入力内容の消去をします。
* お気に入りの削除は削除したいお気に入りを選択して右クリックから削除です。

# 現在出来ること
動画配信サイトから配信一覧を取得して表示
お気に入りに設定した検索語で配信一覧から検索し、合うものがあったら通知
…以上！

# 要望など
GitHubのIssueかマシュマロにどうぞ

[https://marshmallow-qa.com/morohe_qa](https://marshmallow-qa.com/morohe_qa)

# 謝辞
STACKOVERFLOWやQiitaにはお世話になりました。あとReactivePropertyとても使いやすいです。
ライブラリの作者の方々に感謝

# TODO
* PixivSketch以外の配信サイトへの対応
* DLLのプラグイン化
* 開くブラウザやらプログラムの設定
* 通知クリックで配信を開く
* ログの保存
* UIに表示している文字列のローカライズリソースへの追い出し
