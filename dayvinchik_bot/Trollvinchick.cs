using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VkNet.AudioBypassService.Extensions;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;
using System.Collections;
using System.Threading;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using Timer = System.Threading.Timer;
using VkNet.Model.Attachments;

namespace dayvinchik_bot
{
    //написать очередь для сообщений пользователя (victim)
    //реализовать авторизацию
    //исправить баг с закрытием вкладок
    //подумать как создать автоустановку браузера mozilla и его движка
    //реализовать ввод id группы и id данного аккаунта через парсинг ссылки
    public partial class Trollvinchick : Form
    {
        public static VkApi api;
        public static long? id, conv_id;
        public static string? login, password;
        static Random r;
        Timer timer, timer2, timer3, timer4;
        static Queue<VkNet.Model.Message> soobshenia;
        static Queue<string> soobshenia_neyro;
        static Queue<string> show;
        public static bool Neyro_enabled;
        public CancellationToken cancel;
        public static string MESSAGE;
        public static IWebDriver driver;
        public static bool NAME;
        public Trollvinchick()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            r = new Random();
            ServiceCollection services = new ServiceCollection();
            services.AddAudioBypass();
            api = new VkApi(services);
            label1.Text = "";
            soobshenia = new Queue<VkNet.Model.Message>();
            soobshenia_neyro = new Queue<string>();
            show = new Queue<string>();
            Neyro_enabled = false;
            MESSAGE = "";
            checkBox1.Enabled = false;
            FirefoxOptions option = new FirefoxOptions();
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(System.Windows.Forms.Application.StartupPath.Substring(0, System.Windows.Forms.Application.StartupPath.LastIndexOf("dayvinchik_bot") + 15));
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;
            option.AddArguments("--disable-extensions");
            option.AddArgument("--ignore-certificate-errors");
            option.AddArguments("--headless");
            driver = new FirefoxDriver(service, option);
            driver.Navigate().GoToUrl("http://p-bot.ru/");
            richTextBox1.ReadOnly = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Старт")
            {
                try
                {
                    if (textBox6.Text != "" && textBox6.Text != null) id = long.Parse(textBox6.Text);
                    if (textBox3.Text != "" && textBox3.Text != null) login = textBox3.Text;
                    if (textBox2.Text != "" && textBox2.Text != null) password = textBox2.Text;
                    if (textBox5.Text != "" && textBox5.Text != null) conv_id = long.Parse(textBox5.Text);
                }
                catch { }
                api.Authorize(new ApiAuthParams
                {
                    Login = login,
                    Password = password
                });
                button1.Text = "Стоп";
                TimerCallback tm = new TimerCallback(Get_send);
                timer = new Timer(tm, 2, 0, 1000);
                TimerCallback tm2 = new TimerCallback(Send_to_person);
                timer2 = new Timer(tm2, 2, 0, 3000);
                TimerCallback tm3 = new TimerCallback(Bot);
                timer3 = new Timer(tm3, 2, 0, 3000);
                TimerCallback tm4 = new TimerCallback(Showing);
                timer4 = new Timer(tm4, 2, 0, 1000);
                checkBox1.Enabled = true;
                textBox6.Enabled = false;
                textBox3.Enabled = false;
                textBox2.Enabled = false;
            }
            else
            {
                if (timer != null && timer2 != null && timer3 != null && timer4 != null)
                {
                    timer.Dispose();
                    timer2.Dispose();
                    timer3.Dispose();
                    timer4.Dispose();
                }
                button1.Text = "Старт";
                Neyro_enabled = false;
                try
                {
                    if (driver != null) driver.Quit();
                }
                catch (Exception ex)
                {
                    label1.Text = ex.Message;
                }
                if (checkBox1.Checked == true) checkBox1.Checked = false;
                checkBox1.Enabled = false;
                textBox6.Enabled = true;
                textBox3.Enabled = true;
                textBox2.Enabled = true;
            }
        }

        public static void Send_to_person(object obj)
        {
            if (soobshenia.Count > 0)
            {
                var m = soobshenia.Dequeue();
                if (m.Emoji == true)
                {
                    api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        RandomId = r.Next(100, 1000000), // уникальный
                        UserId = id,
                        StickerId = uint.Parse(m.Text)
                    });
                }
                else if (m.Text != "" && m.Text != null)
                {
                    api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        RandomId = r.Next(100, 1000000), // уникальный
                        UserId = id,
                        Message = m.Text
                    });
                }
                if (m.ForwardedMessages != null && m.ForwardedMessages.Count > 0)
                {
                    List<long> a = new List<long>();
                    foreach (var i in m.ForwardedMessages)
                    {
                        a.Add((long)i.Id);
                    }
                    api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        RandomId = r.Next(100, 1000000), // уникальный
                        UserId = id,
                        ForwardMessages = a
                    });
                }
                try
                {
                    if (m.Attachments != null && m.Attachments.Count > 0)
                    {
                        List<VkNet.Model.Attachments.MediaAttachment> a = new List<VkNet.Model.Attachments.MediaAttachment>();
                        foreach (var i in m.Attachments)
                        {
                            a.Add(i.Instance);
                        }
                        api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                        {
                            RandomId = r.Next(100, 1000000), // уникальный
                            UserId = id,
                            Attachments = a
                        });
                    }
                }
                catch { }
                if (m.Geo != null)
                {
                    api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        RandomId = r.Next(100, 1000000), // уникальный
                        UserId = id,
                        Lat = m.Geo.Coordinates.Latitude,
                        Longitude = m.Geo.Coordinates.Longitude,
                    });
                }
            }
        }
        public void Bot(object obj)
        {
            if (soobshenia_neyro.Count > 0 && Neyro_enabled == true)
            {
                MainAsync(soobshenia_neyro.Dequeue()).Wait();
                if (MESSAGE != "" && MESSAGE != null)
                {
                    api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        RandomId = r.Next(100, 1000000), // уникальный
                        ChatId = 1,
                        Message = "BOT: " + MESSAGE
                    });
                    api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        RandomId = r.Next(100, 1000000), // уникальный
                        UserId = id,
                        Message = MESSAGE
                    });
                    show.Enqueue("BOT: " + MESSAGE);
                    MESSAGE = "";
                }
            }
        }
        public void Showing(object obj)
        {
            if (show.Count > 0)
            {
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    richTextBox1.Text = richTextBox1.Text + show.Dequeue() + "\n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    richTextBox1.Refresh();
                });
            }
        }
        public void Get_send(object obj)
        {
            long? m = null;
            string name = "";
            var mes = api.Messages.GetConversations(new VkNet.Model.RequestParams.GetConversationsParams { });
            foreach (var prof in mes.Items)
            {
                if (prof.Conversation.Peer.Id == id)
                {
                    if (prof.Conversation.UnreadCount > 0)
                    {
                        var mes2 = api.Messages.GetConversationMembers(prof.Conversation.Peer.Id);
                        foreach (var i in mes2.Profiles)
                        {
                            if (i.Id == id)
                            {
                                name = i.FirstName + " " + i.LastName + ": ";
                                break;
                            }
                        }
                        m = prof.LastMessage.Id;
                        if (prof.LastMessage.Text != "" && prof.LastMessage.Text != null)
                        {
                            show.Enqueue("(ОБЪЕКТ) " + name + prof.LastMessage.Text);
                            bool flag = false;
                            foreach (char i in prof.LastMessage.Text)
                            {
                                if (Char.IsLetterOrDigit(i))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == true)
                            {
                                soobshenia_neyro.Enqueue(prof.LastMessage.Text);
                            }
                        }
                        if(prof.LastMessage.Attachments != null && prof.LastMessage.Attachments.Count > 0)
                        {
                            show.Enqueue("(ОБЪЕКТ) " + name + "(вложение)");
                        }
                        api.Messages.MarkAsRead(prof.Conversation.Peer.Id.ToString());
                    }
                    break;
                }
            }
            var mes3 = api.Messages.GetConversations(new VkNet.Model.RequestParams.GetConversationsParams { });
            foreach (var prof in mes3.Items)
            {
                if (prof.Conversation.Peer.Id == 2000000000 + conv_id)
                {
                    if (prof.Conversation.UnreadCount > 0)
                    {
                        soobshenia.Enqueue(prof.LastMessage);
                        string name2 = "";
                        var mes4 = api.Messages.GetConversationMembers(prof.Conversation.Peer.Id);
                        foreach (var i in mes4.Profiles)
                        {
                            if (i.Id == prof.LastMessage.FromId)
                            {
                                name2 = i.FirstName + " " + i.LastName + ": ";
                                break;
                            }
                        }
                        if (prof.LastMessage.Text != "" && prof.LastMessage.Text != null)
                        {
                            show.Enqueue(name2 + prof.LastMessage.Text);
                        }
                        if (prof.LastMessage.Attachments != null && prof.LastMessage.Attachments.Count > 0)
                        {
                            show.Enqueue(name2 + "(вложение)");
                        }
                        api.Messages.MarkAsRead(prof.Conversation.Peer.Id.ToString());
                    }
                    break;
                }
            }
            if (m != null)
            {
                List<long> a = new List<long>();
                a.Add((long)m);
                api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                {
                    RandomId = r.Next(100, 1000000), // уникальный
                    ChatId = 1,
                    ForwardMessages = a
                });
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer != null && timer2 != null && timer3 != null && timer4 != null)
            {
                timer.Dispose();
                timer2.Dispose();
                timer3.Dispose();
                timer4.Dispose();
            }
            Neyro_enabled = false;
            try
            {
                if (driver != null)
                {
                    driver.Quit();
                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            soobshenia.Clear();
            soobshenia_neyro.Clear();
        }
        private static async Task MainAsync(string message)
        {
            if (NAME == false)
            {
                driver.FindElement(By.Id("labelUserName")).Click();
                await Task.Delay(50);
                string name = "";
                var mes = api.Messages.GetConversationMembers((long)id);
                foreach (var i in mes.Profiles)
                {
                    if (i.Id == id)
                    {
                        name = i.FirstName;
                        break;
                    }
                }
                var q = driver.FindElement(By.Id("userNameInput"));
                q.Clear();
                await Task.Delay(100);
                q.SendKeys(name);
                await Task.Delay(100);
                driver.FindElement(By.Id("btnSaveAnswer_formUserName")).Click();
                NAME = true;
            }
            driver.FindElement(By.Id("user_request")).Clear();
            await Task.Delay(50);
            driver.FindElement(By.Id("user_request")).SendKeys(message);
            await Task.Delay(100);
            driver.FindElement(By.Id("btnSay")).Click();
            await Task.Delay(2000);
            do
            {
                await Task.Delay(300);
                if(driver.FindElement(By.Id("answer_0")) != null)
                {
                    if (driver.FindElement(By.Id("answer_0")).Text.Contains("Думаю...")) continue;
                }
            } while (driver.FindElement(By.Id("answer_0")) == null);
            MESSAGE = driver.FindElement(By.Id("answer_0")).Text.Replace("ρBot: ", "");
            await Task.Delay(100);
        }

        private int GetId(string reference)
        {
            return int.Parse(reference.Remove(0, reference.Length - 9));
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)//код с телефона
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)//пароль
        {

        }
        private void textBox3_TextChanged(object sender, EventArgs e)//логин
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)//ссылка на беседу
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)//ссылка на пользователя
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)//neyro on/off
        {
            if (checkBox1.Checked)
            {
                Neyro_enabled = true;
                NAME = false;
            }
            else
            {
                Neyro_enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form.ActiveForm.Close();
        }
    }
}
