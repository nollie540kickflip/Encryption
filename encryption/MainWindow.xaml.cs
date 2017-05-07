using System.Windows;

namespace encryption
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // 暗号化・複合化時に使用するパスワード
        const string password = "hogehoge";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 複合ファイルドラッグ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFuku_PreviewDragOver(object sender, DragEventArgs e)
        {
            // マウスカーソルをコピーにする。
            e.Effects = DragDropEffects.Copy;
            // ドラッグされてきたものがFileDrop形式の場合だけ、このイベントを処理済みにする。
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        /// <summary>
        /// 複合ファイルドロップ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFuku_Drop(object sender, DragEventArgs e)
        {
            txtFuku.Text = string.Empty;

            // ドロップされたものがFileDrop形式の場合は、各ファイルのパス文字列を文字列配列に格納する。
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
            {
                txtFuku.Text = files[0];
            }
        }

        /// <summary>
        /// 暗号ファイルドラッグ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAngo_PreviewDragOver(object sender, DragEventArgs e)
        {
            // マウスカーソルをコピーにする。
            e.Effects = DragDropEffects.Copy;
            // ドラッグされてきたものがFileDrop形式の場合だけ、このイベントを処理済みにする。
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        /// <summary>
        /// 暗号ファイルドロップ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAngo_Drop(object sender, DragEventArgs e)
        {
            txtAngo.Text = string.Empty;

            // ドロップされたものがFileDrop形式の場合は、各ファイルのパス文字列を文字列配列に格納する。
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
            {
                txtAngo.Text = files[0];
            }
        }

        /// <summary>
        /// 複合ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFuku_Click(object sender, RoutedEventArgs e)
        {
            string strAngo = "";
            string strFuku = "";

            if (!InputChk()) return;

            // 暗号ファイル読込
            using (var file = new System.IO.StreamReader(txtAngo.Text.Trim()))
            {
                while (file.EndOfStream == false)
                {
                    foreach (var line in file.ReadLine())
                    {
                        strAngo += line;
                    }
                }
            }

            // 複合化処理
            strFuku = DecryptString(strAngo, password);

            // 複合ファイル書込み
            try
            {
                using (var file = new System.IO.StreamWriter(txtFuku.Text.Trim()))
                {
                    file.WriteLine(strFuku);
                }
            }
            catch (System.IO.IOException)
            {
                ShowMsg("複合ファイル書込み中にエラー発生!!");
            }

            ShowMsg("複合処理が完了しました。");
        }

        /// <summary>
        /// 暗号ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAngo_Click(object sender, RoutedEventArgs e)
        {
            string strFuku = "";
            string strAngo = "";

            if (!InputChk()) return;

            // 複合ファイル読込
            using (var file = new System.IO.StreamReader(txtFuku.Text.Trim()))
            {
                while (file.EndOfStream == false)
                {
                    // 一行読込
                    foreach (var line in file.ReadLine())
                    {
                        strFuku += line;
                    }
                    // 改行追加
                    if (file.EndOfStream == false)
                    {
                        strFuku += System.Environment.NewLine;
                    }
                }
            }

            // 暗号化処理
            strAngo = EncryptString(strFuku, password);

            // 暗号ファイル書込み
            try
            {
                using (var file = new System.IO.StreamWriter(txtAngo.Text.Trim()))
                {
                    file.WriteLine(strAngo);
                }
            }
            catch (System.IO.IOException)
            {
                ShowMsg("暗号ファイル書込み中にエラーが発生!!");
            }

            ShowMsg("暗号処理が完了しました。");
        }

        /// <summary>
        /// 入力値チェック
        /// </summary>
        /// <returns></returns>
        private bool InputChk()
        {
            string inpFuku = txtFuku.Text.Trim();
            string inpAngo = txtAngo.Text.Trim();

            if (inpFuku == "")
            {
                ShowMsg("複合ファイルのパスを入力して下さい!!");
                return false;
            }

            if (inpAngo == "")
            {
                ShowMsg("暗号ファイルのパスを入力して下さい!!");
                return false;
            }

            if (!System.IO.File.Exists(inpFuku))
            {
                ShowMsg("複合ファイルが存在しません!!");
                return false;
            }

            if (!System.IO.File.Exists(inpAngo))
            {
                ShowMsg("暗号ファイルが存在しません!!");
                return false;
            }

            return true;
        }

        private void ShowMsg(string msg)
        {
            MessageBox.Show(msg);
        }

        /// <summary>
        /// 文字列を暗号化する
        /// </summary>
        /// <param name="sourceString">暗号化する文字列</param>
        /// <param name="password">暗号化に使用するパスワード</param>
        /// <returns>暗号化された文字列</returns>
        public static string EncryptString(string sourceString, string password)
        {
            //RijndaelManagedオブジェクトを作成
            System.Security.Cryptography.RijndaelManaged rijndael =
                new System.Security.Cryptography.RijndaelManaged();

            //パスワードから共有キーと初期化ベクタを作成
            byte[] key, iv;
            GenerateKeyFromPassword(
                password, rijndael.KeySize, out key, rijndael.BlockSize, out iv);
            rijndael.Key = key;
            rijndael.IV = iv;

            //文字列をバイト型配列に変換する
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);

            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform encryptor =
                rijndael.CreateEncryptor();
            //バイト型配列を暗号化する
            byte[] encBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
            //閉じる
            encryptor.Dispose();

            //バイト型配列を文字列に変換して返す
            return System.Convert.ToBase64String(encBytes);
        }

        /// <summary>
        /// 暗号化された文字列を復号化する
        /// </summary>
        /// <param name="sourceString">暗号化された文字列</param>
        /// <param name="password">暗号化に使用したパスワード</param>
        /// <returns>復号化された文字列</returns>
        public static string DecryptString(string sourceString, string password)
        {
            //RijndaelManagedオブジェクトを作成
            System.Security.Cryptography.RijndaelManaged rijndael =
                new System.Security.Cryptography.RijndaelManaged();

            //パスワードから共有キーと初期化ベクタを作成
            byte[] key, iv;
            GenerateKeyFromPassword(
                password, rijndael.KeySize, out key, rijndael.BlockSize, out iv);
            rijndael.Key = key;
            rijndael.IV = iv;

            //文字列をバイト型配列に戻す
            byte[] strBytes = System.Convert.FromBase64String(sourceString);

            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform decryptor =
                rijndael.CreateDecryptor();
            //バイト型配列を復号化する
            //復号化に失敗すると例外CryptographicExceptionが発生
            byte[] decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
            //閉じる
            decryptor.Dispose();

            //バイト型配列を文字列に戻して返す
            return System.Text.Encoding.UTF8.GetString(decBytes);
        }

        /// <summary>
        /// パスワードから共有キーと初期化ベクタを生成する
        /// </summary>
        /// <param name="password">基になるパスワード</param>
        /// <param name="keySize">共有キーのサイズ（ビット）</param>
        /// <param name="key">作成された共有キー</param>
        /// <param name="blockSize">初期化ベクタのサイズ（ビット）</param>
        /// <param name="iv">作成された初期化ベクタ</param>
        private static void GenerateKeyFromPassword(string password,
            int keySize, out byte[] key, int blockSize, out byte[] iv)
        {
            //パスワードから共有キーと初期化ベクタを作成する
            //saltを決める
            byte[] salt = System.Text.Encoding.UTF8.GetBytes("saltは必ず8バイト以上");
            //Rfc2898DeriveBytesオブジェクトを作成する
            System.Security.Cryptography.Rfc2898DeriveBytes deriveBytes =
                new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt);
            //.NET Framework 1.1以下の時は、PasswordDeriveBytesを使用する
            //System.Security.Cryptography.PasswordDeriveBytes deriveBytes =
            //    new System.Security.Cryptography.PasswordDeriveBytes(password, salt);
            //反復処理回数を指定する デフォルトで1000回
            deriveBytes.IterationCount = 1000;

            //共有キーと初期化ベクタを生成する
            key = deriveBytes.GetBytes(keySize / 8);
            iv = deriveBytes.GetBytes(blockSize / 8);
        }

    }
}
