using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Configuration;

namespace FormTTP
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
        int width = Convert.ToInt32(ConfigurationManager.AppSettings["width"]);
        int height = Convert.ToInt32(ConfigurationManager.AppSettings["height"]);

        // 接收串口返回的数据
        byte[] receive_Data;
        ///用来转换图片的数组        
        byte[] new_ReceiveData;
        int receive_Num = 0;
        //true 开始导入到receive_Data  反之等待触发信号
        //bool flag = false;
        /// <summary>
        /// 串口接收数据
        /// </summary>
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //定义临时数组
            byte[] temp = new byte[serialPort1.BytesToRead];
            //将缓存的数据读取到temp临时数组
            int num = serialPort1.Read(temp, 0, serialPort1.BytesToRead);
            #region 旧的
            //try
            //{
            //    //    //缓存收到的数据
            //    //正在接收
            //    if (flag)
            //    {
            //        //将接收到的byte数组复制到receive_Data
            //        if (temp.Length < 240 * 240 + 4 - receive_Num - 1)
            //        {
            //            Array.Copy(temp, 0, receive_Data, receive_Num, temp.Length);
            //            receive_Num += temp.Length;
            //        }
            //        else
            //        {
            //            Array.Copy(temp, 0, receive_Data, receive_Num, 240 * 240 + 4 - receive_Num - 1);
            //            receive_Num = 240 * 240 + 3;
            //            TextToBmp();
            //            flag = false;
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < temp.Length; i++)
            //        {
            //            //true 触发开始接收数据
            //            if (temp[i] == 0xff && temp[i + 1] == 0xff)
            //            {
            //                receive_Data = new byte[240 * 241];
            //                if (temp.Length - i < 240 * 240 + 4)
            //                {
            //                    Array.Copy(temp, 0, receive_Data, 0, temp.Length - i);
            //                    receive_Num += temp.Length - i - 1;
            //                    flag = true;
            //                }
            //                else
            //                {
            //                    Array.Copy(temp, i, receive_Data, 0, 240 * 240 + 4);
            //                    receive_Num = 240 * 240 + 3;
            //                    TextToBmp();
            //                    flag = false;
            //                }
            //                break;
            //            }
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    label4.Text = "接收数据异常，联系你爸爸.异常原因：" + ex.ToString();
            //}
            #endregion
            #region 单独一次接收
            if (num > 0)
            {
                //将临时数组的数据复制到receive_Data
                Array.Copy(temp, 0, receive_Data, receive_Num, temp.Length);
                receive_Num += temp.Length;
                //如果接收的数据已大于图片总像素开始转换
                if (receive_Num >= width * height + 3)
                {
                    TextToBmp();
                }
            }
            #endregion
        }

        /// <summary>
        /// 将接收到的数据转换为灰度图片
        /// </summary>
        void TextToBmp()
        {
            //DateTime dt = DateTime.Now;
            //避免结尾receive_Data初始化会影响新的数据，赋值到new_ReceiveData
            new_ReceiveData = receive_Data;
            receive_Num = 0;
            receive_Data = new byte[width * height + 4];
            //文本测试使用
            //string[] strdata = File.ReadAllText(Application.StartupPath + @"\New File.txt").Split(' ');
            //如果首尾标志正确开始解析
            if (new_ReceiveData[0] == 0xff && new_ReceiveData[1] == 0xff &&
                new_ReceiveData[width * height + 2] == 0xff && new_ReceiveData[width * height + 3] == 0xff)
            {
                int gray;
                Bitmap bitmap = new Bitmap(width, height);
                Bitmap bitmap2 = new Bitmap(width, height);
                //从数组下标 2 开始为像素点
                int m = 2;
                MyMath myMath = new MyMath(width, height);
                byte[] mathData = new byte[240 * 240];
                //mathdata是只有像素点，没有0xff标志
                Array.Copy(new_ReceiveData, 2, mathData, 0, 240 * 240);
                //文本测试使用
                //Array.Copy(strdata, 2, ddda, 0, 240 * 240);
                //求出阈值
                int thre = myMath.threshold(mathData);
                lab_threshold.Text = thre.ToString();
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        //文本测试使用
                        //gray = Convert.ToInt32(strdata[m], 16) * 255 / 96;
                        gray = new_ReceiveData[m] * 255 / 96;
                        Color newColor = Color.FromArgb(gray, gray, gray);
                        bitmap.SetPixel(i, j, newColor);
                        // 文本测试使用
                        // gray = Convert.ToInt32(strdata[m], 16) > thre ? 255 : 0;
                        gray = new_ReceiveData[m] > thre ? 255 : 0;
                        newColor = Color.FromArgb(gray, gray, gray);
                        bitmap2.SetPixel(i, j, newColor);
                        m++;
                    }
                }
                pictureBox1.Image = bitmap;
                pictureBox2.Image = bitmap2;
            }
            else
            {
                //数据有误，创建文件流 模式为创建test文本 写入数据
                FileStream fileStream = new FileStream(Application.StartupPath + @"\test.txt", FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(String.Format("当前已接收长度：{0}", receive_Num));
                fileStream.Write(new_ReceiveData, 0, new_ReceiveData.Length);
                //关闭流
                streamWriter.Close();
                fileStream.Close();
                label4.Text = "TextToBmp :数据转换有误,数据已保存在text.txt";
            }
            new_ReceiveData = new byte[width * height + 4];
            //DateTime lastTime = DateTime.Now;
            //int time = (lastTime - dt).Milliseconds;
            //Console.WriteLine(time);
        }

        #region 开关串口
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Open_Click(object sender, EventArgs e)
        {
            try
            {
                //combox不为null 打开串口
                if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null)
                {
                    //端口
                    serialPort1.PortName = "COM" + comboBox1.SelectedItem.ToString();
                    //波特率
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.SelectedItem.ToString());
                    serialPort1.Open();
                    btn_Open.Enabled = false;
                    btn_Close.Enabled = true;
                    BeginInvoke(new Action(() =>
                    {
                        label8.Text = "串口已打开";
                        label8.ForeColor = Color.Green;
                    }));
                }
                else
                {
                    label4.Text = "打开串口失败，请检查配置";
                    label4.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                label4.Text = "打开串口异常。异常原因：" + ex.ToString();
                label4.ForeColor = Color.Red;
            }
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            //释放资源
            serialPort1.Dispose();
            serialPort1.Close();
            //初始化接收数组
            receive_Num = 0;
            receive_Data = new byte[width * height + 4];
            btn_Close.Enabled = false;
            btn_Open.Enabled = true;
            BeginInvoke(new Action(() =>
            {
                label8.Text = "串口已关闭";
                label8.ForeColor = Color.Gray;
            }));
        }
        #endregion

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 4;
            comboBox2.SelectedIndex = 6;
            textBox1.Text = width.ToString();
            textBox3.Text = height.ToString();
            receive_Data = new byte[width * height + 4];
            new_ReceiveData = new byte[width * height + 4];
        }

        private void btn_PicSet_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox3.Text != "")
            {
                width = Convert.ToInt32(textBox1.Text);
                height = Convert.ToInt32(textBox3.Text);
                //更改配置文件
                Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                cfa.AppSettings.Settings["width"].Value = width.ToString();
                cfa.AppSettings.Settings["height"].Value = height.ToString();
                cfa.Save(ConfigurationSaveMode.Modified);
                //刷新，否则程序读取的还是之前的值（可能已装入内存）
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                label4.Text = "图像配置已更改成功";
                label4.ForeColor = Color.Green;
            }
            else
            {
                label4.Text = "图像配置已更改失败";
                label4.ForeColor = Color.Red;
            }
        }
    }
}
