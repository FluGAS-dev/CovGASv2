## **<u>CovGAS </u>**





### これは何 

CovGAS：Corona Virus Genome Assembly and subtyping　

高速シーケンスされたサンプル情報からNCBIに登録されているCorona株を参照し、シーケンスデータよりゲノムコンセンサス配列を作成することを目的とするツールです。



### CovGASの特徴

通常はLinux上のコマンドラインで行ってるマッピングなどのコマンドをWindows上のGUIからコマンドを入力することなく自動的に実行されます。実行されるワークフローによりコマンドを扱うよりも時間をかけずに結果を取得できます。

結果はコンセンサス配列と共にNCBIに登録されている近似株の情報を得ることが出来ます。

コンセンサス配列を作成する処理はFluGASの機構に準拠します。





### CovGASの機能

ユーザが意識することなく敵弁なCPU Core数を用いて処理を行います。

シーケンスデータから次項のワークフローを用いて、コロナウイルスのコンセンサス塩基配列を作成します。

作成されたコロナウイルスのコンセンサス配列が、NCBIに登録されているコロナウイルスに近似しているかを表示します。





### システム要件

OS：Windows10　pro（1909以降）IntelCPU版

CPU：DualCore以上　（Intel Core i5 Skylake 以降を推奨）

メモリ：8GB以上　（入力データ量に準ずる　Illuminaシーケンサ　ペアエンド 150Mx2 ）

ストレージ：インストール 2GB　 実行時 入力データサイズの2～10倍程度





### 機能要件

ユーザインタフェース部分はC# （WPF）を使用します。

ユーザがシーケンスデータを選択しやすいインターフェースを作成します。

外部コマンドはできうる限りOSSを利用し、非OSSが必要な場合はユーザが任意でインストールを行えるよう促せるようにします。

外部ツールのOSSはMinGW等のコンパイラにてWindowsバイナリーをあらかじめ用意しCovGASに含めます。

Windowsバイナリーが用意できない場合は、WSL（Windows Subsystem Linux）経由でLinux版のバイナリーファイルを扱うように設定します。

前述OSS利用時のパラメータはデフォルト値をあらかじめセットします。またユーザが任意にコマンドオプションを設定可能なインターフェースを作成します。

解析結果を表示する機能を作成します。

解析結果を出力する機能を作成します。





### 拡張性

作成された機能が利用するツールが日々更新され、また新規ツールが開発されています。のちに外部ツールの変更が容易なように既存のフレームワークを利用し改修・変更を行いやすい設計とします。

こんソフトウエアではWPFのOSSフレームワークLivetを利用します。

またソフトウエア内で利用している外部ツールを呼び出すAPIは専用のDLLを使用します。（外部コマンドの呼び出しとユーザインタフェースを区別します）





### 機能フロー

###### 入力ファイル

入力はIllumina シーケンサのFastqファイル（Single/Pair）、NanoporeシーケンサのFast5もしくはBasecall済みのFastqファイルを対象とします。

※Nanoporeシーケンサ Fast5ファイルを入力とする場合、別途Basecallerが含まれるGuppyが必要になります。GuppyはWindows CPU版を事前にインストールすることでCovGASから使用されます。



###### Illuminaシーケンサ WorkFlow

CovGAS実行画面よりIlluminaシーケンサであることを指定しファイルを選択します。ファイルの選択はWindows エクスプローラーから選択しDrag&Dropを用いて複数サンプルを指定することを可能にします。

シーケンスしたサンプルがシングルエンドまたはペアエンドは以下のネーミングルールによって自動的に判別します。

　-- ペアエンドとして認識する命名規則

```
 	ファイル名を”L001” で分割した前方が一致するファイルが2つある場合
 	例）sample_name_L001_R1_001.fastq.gz
 	   sample_name_L001_R2_001.fastq.gz

	ファイル名を”--”で分割した前方が一致するファイルが2つある場合
	例）sample_name--001.fastq.gz
 	   sample_name--002.fastq.gz

```



複数サンプルを入力した場合、サンプル単位で以下の処理が順次実行します。

・Trimmomatic（http://www.usadellab.org/cms/?page=trimmomatic）を使用しサンプル配列QCを行い、QCの通ったものを次のマッピング対象とします。

・bowtie2（http://bowtie-bio.sourceforge.net/bowtie2/index.shtml）を使用し前項QCを通過したFastqファイルを対象にNCBIより取得したリファレンス配列へマッピングを行います。

・カバー率・カバレッジの良かったマッピング結果（SAM）より、samtools（http://www.htslib.org/）を使用しコンセンサス配列を作成（1st-Consensus）を作成します。

・コンセンサス配列をQueryとし、NCBIに登録されているCorona virusの配列に対してBLAST（https://blast.ncbi.nlm.nih.gov/）を実行し1st-Consensus近似配列を選択し次項のマッピングリファレンスとします。

・前項で選出したCorona virus塩基配列（1-fasta）をリファレンスにしてbowtie2を使用しマッピングを行います。

・1エントリーのReferenceを対象に行ったマッピング結果（SAM）より、samtoolsを使用しコンセンサス配列（2nd-Consensus）を作成します。





###### Nanoporeシーケンサ WorkFlow

CovGAS実行画面よりNanoporeシーケンサであることを指定しフォルダもしくはファイルを選択します。ファイルの選択はWindows エクスプローラーから選択しDrag&Dropを用いて複数サンプルを指定することを可能にします。

※フォルダが指定された場合、指定されたフォルダ内にあるFast5が対象になります。Fast5はGuppyのbasecallerでfastqへ変換されます。



・Fast5ファイルが指定された場合、basecallerを利用しFastq形式へ自動的に変換します。

・Fast5ファイルが指定された場合、Fastqの変換後にbarcode分割を行います。（ただしユーザが単一サンプルのシーケンスとした場合はbarcode分割の処理を行いません）

・minimap2（https://github.com/lh3/minimap2）を使用し、選択されたFastq、もしくは上記処理で作成されたFastqファイルを対象にNCBIより取得したリファレンス配列へマッピングを行います。

・カバー率・カバレッジの良かったマッピング結果（SAM）より、samtools（http://www.htslib.org/）を使用しコンセンサス配列を作成（1st-Consensus）を作成します。

・コンセンサス配列をQueryとし、NCBIに登録されているCorona virusの配列に対してBLAST（https://blast.ncbi.nlm.nih.gov/）を実行し1st-Consensus近似配列を選択し次項のマッピングリファレンスとします。

・前項で選出したCorona virus塩基配列（1-fasta）をリファレンスにしてbowtie2を使用しマッピングを行います。

・1エントリーのReferenceを対象に行ったマッピング結果（SAM）より、samtoolsを使用しコンセンサス配列（2nd-Consensus）を作成します。





###### 使用ツール

・samtools（http://www.htslib.org/）

・Trimmomatic（http://www.usadellab.org/cms/?page=trimmomatic）

・bowtie2（http://bowtie-bio.sourceforge.net/bowtie2/index.shtml）

・BLAST（https://blast.ncbi.nlm.nih.gov/）

・minimap2（https://github.com/lh3/minimap2）





### データの永続性

本ツールでは実行したサンプルの情報を保持します。

また解析時に使用するパラメータも保存し次回以降に同じパラメータで実行できるように構成します。



保存情報はSQLiteを利用しユーザインタフェースへ読み込み表示します。

ただし解析結果はストレージ容量が不足することから本ツールでは保存・管理を行いません。

解析結果に付随するQC後Fastaq、マッピング結果等を削除した場合に結果ファイルを用いたビュアーは機能しなくなります。







### フロー図

![IlluminaFlow](C:\Users\nak\source\repos\CovGASv2\Doc\IlluminaFlow.png)





![nanoporeFlow](C:\Users\nak\source\repos\CovGASv2\Doc\nanoporeFlow.png)





### 画面イメージ



![exec_screen](C:\Users\nak\source\repos\CovGASv2\Doc\exec_screen.PNG)





![kaiseki_kekka](C:\Users\nak\source\repos\CovGASv2\Doc\kaiseki_kekka.PNG)