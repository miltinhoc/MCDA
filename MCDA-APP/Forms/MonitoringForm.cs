﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;

namespace MCDA_APP.Forms
{
    public partial class MonitoringForm : Form
    {
        double minThreatScore = 15.0;
        bool sendStatistics = true;
        bool closing = false;

        public MonitoringForm()
        {
            InitializeComponent();

            if (!Directory.Exists(@"./malcore"))
            {
                Directory.CreateDirectory(@"./malcore");
            }
            if (!Directory.Exists(@"./malcore/threat"))
            {
                Directory.CreateDirectory(@"./malcore/threat");
            }
            if (!Directory.Exists(@"./malcore/doc"))
            {
                Directory.CreateDirectory(@"./malcore/doc");
            }
        }

        private async void MonitoringForm_Load(object sender, EventArgs e)
        {
            this.Visible = true;

            try
            {
                // check if active or inactive
                bool active =  await agentUsuage();

                if (active)
                {
                    panelInactive.Visible = false;
                    monitoringFlowLayoutPanel.Visible = true;
                    labelRemaining.Visible = true;
                    lblRequestNumber.Visible = true;
                    lblStatus.Text = "ACTIVE";
                    lblStatus.ForeColor = Color.Green;

                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@".malcore");
                    if (key != null)
                    {
                        var SETTINGS = key.GetValue("SETTINGS");
                        if (SETTINGS != null)
                        {
                            JObject json = JObject.Parse(SETTINGS.ToString());
                            this.minThreatScore = (double)(json["minThreatScore"]);
                            this.sendStatistics = (bool)(json["sendStatistics"]);

                            if ((bool)json["enableMornitoring"])
                            {
                                // start monitoring
                                string settings_paths = (string)json["paths"];
                                // this.paths = settings_paths.Split(',').ToList();

                                List<string> paths = settings_paths.Split(',').ToList();
                                int num = 0;
                                foreach (string path in paths)
                                {
                                    if (File.Exists(path))
                                    {
                                        ProcessFile(path);
                                    }
                                    else if (Directory.Exists(path))
                                    {
                                        ProcessDirectory(path);
                                        switch (num)
                                        {
                                            case 0:
                                                fileSystemWatcherMain.Path = path;
                                                break;
                                            case 1:
                                                fileSystemWatcher1.Path = path;
                                                break;
                                            case 2:
                                                fileSystemWatcher2.Path = path;
                                                break;
                                            case 3:
                                                fileSystemWatcher3.Path = path;
                                                break;
                                            case 4:
                                                fileSystemWatcher4.Path = path;
                                                break;
                                            case 5:
                                                fileSystemWatcher5.Path = path;
                                                break;
                                            case 6:
                                                fileSystemWatcher6.Path = path;
                                                break;
                                            case 7:
                                                fileSystemWatcher7.Path = path;
                                                break;
                                            case 8:
                                                fileSystemWatcher8.Path = path;
                                                break;
                                            case 9:
                                                fileSystemWatcher9.Path = path;
                                                break;
                                            case 10:
                                                fileSystemWatcher10.Path = path;
                                                break;
                                            case 11:
                                                fileSystemWatcher11.Path = path;
                                                break;
                                            case 12:
                                                fileSystemWatcher12.Path = path;
                                                break;
                                            case 13:
                                                fileSystemWatcher13.Path = path;
                                                break;
                                            case 14:
                                                fileSystemWatcher14.Path = path;
                                                break;
                                            case 15:
                                                fileSystemWatcher15.Path = path;
                                                break;
                                            case 16:
                                                fileSystemWatcher16.Path = path;
                                                break;
                                            case 17:
                                                fileSystemWatcher17.Path = path;
                                                break;
                                            case 18:
                                                fileSystemWatcher18.Path = path;
                                                break;
                                            case 19:
                                                fileSystemWatcher19.Path = path;
                                                break;
                                            default:
                                                fileSystemWatcherMain.Path = path;
                                                break;
                                        }

                                        num++;
                                    }
                                    else
                                    {
                                        // Console.WriteLine("{0} is not a valid file or directory.", path);
                                    }

                                }
                            }
                        }
                    }
                }
                else
                {
                    panelInactive.Visible = true;
                    monitoringFlowLayoutPanel.Visible = false;
                    labelRemaining.Visible = false;
                    lblRequestNumber.Visible = false;
                    lblStatus.Text = "INACTIVE";
                    lblStatus.ForeColor = Color.Red;
                }


            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine(ex);
            }
        }


        private async Task<string> getThreatScore(string pathFile, string fileName, string type)
        {
            try
            {
                string payload = "{\"type\":\"file_submitted\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"" + type + "\",\"message\":\"File submitted\"}}";
                await agentStat(payload);

                handleRelease(pathFile, false);
                string url = System.Configuration.ConfigurationManager.AppSettings["URI"] + "/api/" + type;

                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        byte[] fileData = File.ReadAllBytes(pathFile);
                        content.Add(new StreamContent(new MemoryStream(fileData)), "filename1", fileName);
                        content.Headers.Add("apiKey", Program.APIKEY);
                        content.Headers.Add("source", "agent");

                        using (
                           var response = await client.PostAsync(url, content))
                        {
                            handleRelease(pathFile, true);

                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string responseString = await response.Content.ReadAsStringAsync();
                                return responseString;
                            }
                            else
                            {
                                string payload2 = "{\"type\":\"file_failed\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"" + type + "\",\"response\":\"api_error\",\"message\":\"API Error\"}}";
                                await agentStat(payload2);
                                return "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine("getThreatScore Exception.........................." + ex);
                handleRelease(pathFile, false);

                string payload = "{\"type\":\"file_failed\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"" + type + "\",\"response\":\"timeout/api_error/other_error\",\"message\":\"API Error/Timeout/Other\"}}";
                await agentStat(payload);
                return "";
            }
        }

        private async Task<string> agentStat(string jsonData)
        {
            try
            {
                if (this.sendStatistics == false)
                {
                    return "";
                }
                string url = System.Configuration.ConfigurationManager.AppSettings["URI"] + "/agent/stat";

                using (var client = new HttpClient())
                {
                    // string jsonData = "{\"type\":\"started\",\"payload\":{\"message\":\"Agent Started\"}}";

                    var requestContent = new StringContent(jsonData, Encoding.Unicode, "application/json");
                    client.DefaultRequestHeaders.Add("apiKey", Program.APIKEY);
                    client.DefaultRequestHeaders.Add("source", "agent");
                    client.DefaultRequestHeaders.Add("agentVersion", "1.0");

                    using (
                          var response = await client.PostAsync(url, requestContent))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            return content;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine("agentStat dug.........................." + ex);
                return "";
            }
        }

        private async Task<bool> agentUsuage()
        {
            try
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["URI"] + "/agent/usage";

                using (var client = new HttpClient())
                {

                    var requestContent = new StringContent("", Encoding.Unicode, "application/json");
                    client.DefaultRequestHeaders.Add("apiKey", Program.APIKEY);
                    client.DefaultRequestHeaders.Add("source", "agent");
                    client.DefaultRequestHeaders.Add("agentVersion", "1.0");

                    using (
                          var response = await client.PostAsync(url, requestContent))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            JObject jsonObject = JObject.Parse(content);

                            if ((bool)jsonObject["success"] == true)
                            {
                                lblRequestNumber.Text = (string)jsonObject["data"]["remaining"];
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine("agentStat dug.........................." + ex);
                return false;
            }
        }

        private void addItemToMonitoringPanel(string responseString, string folderName, string fileName, bool succeed)
        {
            bool success = false;
            string score = "";
            double score_num = 0;

            if (succeed == true)
            {
                JObject jsonObject = JObject.Parse(responseString);

                success = (bool)jsonObject["success"];
                if (jsonObject["data"]["data"].ToString() == "{}")
                {
                    success = false;
                }

                if (success)
                {
                    success = (bool)jsonObject["data"]["success"];
                    if (success)
                    {
                        score = (string)jsonObject["data"]["data"]["score"];
                        string[] scores = score.Split('/');
                        score_num = Convert.ToDouble(scores[0]);
                        score = Convert.ToString(Math.Round(score_num)) + "%";
                    }
                }
            }

            // item panel
            Panel panel = new Panel();
            panel.Width = 501;
            panel.Height = 44;
            panel.BackColor = Color.Black;

            // color panel
            Panel colorPanel = new Panel();
            colorPanel.Name = "colorPanel";
            colorPanel.Width = 20;
            colorPanel.Height = 44;
            // colorPanel.BackColor = Color.Red;

            // file label
            Label fileLabel = new Label();
            fileLabel.Name = "fileLabel";
            fileLabel.Font = new Font("Calibri", 12, FontStyle.Bold);
            fileLabel.AutoSize = false;
            fileLabel.Width = 200;
            fileLabel.Location = new System.Drawing.Point(22, 1);
            fileLabel.Click += delegate (object obj, EventArgs ea)
            {
                DetailsForm detailsForm = new DetailsForm("threat", responseString, folderName, fileName, this, panel);
                detailsForm.Show(this);
            };

            // folder label
            Label folderLabel = new Label();
            folderLabel.Name = "folderLabel";
            folderLabel.Text = folderName;
            folderLabel.ForeColor = Color.White;
            folderLabel.Font = new Font("Calibri", 11, FontStyle.Regular);
            folderLabel.AutoSize = false;
            folderLabel.Width = 200;
            folderLabel.Location = new System.Drawing.Point(24, 22);
            folderLabel.Click += delegate (object obj, EventArgs ea)
            {
                DetailsForm detailsForm = new DetailsForm("threat", responseString, folderName, fileName, this, panel);
                detailsForm.Show(this);
            };

            // percent label
            Label percentLabel = new Label();
            percentLabel.Name = "percentLabel";
            percentLabel.Text = score;
            percentLabel.Font = new Font("Calibri", 20, FontStyle.Bold);
            percentLabel.Width = 76;
            percentLabel.Height = 40;
            percentLabel.Location = new System.Drawing.Point(252, 4);

            Button removeButton = new Button();
            removeButton.Name = "removeButton";
            removeButton.Text = "DELETE";
            removeButton.Font = new Font("Calibri", 12, FontStyle.Bold);
            removeButton.BackColor = Color.Red;
            removeButton.ForeColor = Color.White;
            removeButton.FlatStyle = FlatStyle.Flat;
            removeButton.FlatAppearance.BorderSize = 0;
            removeButton.Width = 85;
            removeButton.Height = 31;
            removeButton.Location = new System.Drawing.Point(325, 6);
            removeButton.Click += delegate (object obj, EventArgs ea)
            {
                if (File.Exists(folderName + "\\" + fileName))
                {
                    handleRelease(folderName + "\\" + fileName, false);
                    File.Delete(folderName + "\\" + fileName);
                }
                string hashFileName = folderName.Replace("\\", "-").Replace(":", "") + fileName + "-hash.json";
                if (File.Exists("./malcore/threat/" + hashFileName))
                {
                    File.Delete("./malcore/threat/" + hashFileName);
                }
                panel.Dispose();

                string payload = "{\"type\":\"file_deleted\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"threatscore\",\"message\":\"File deleted\"}}";
                agentStat(payload);
            };

            Button rerunButton = new Button();
            rerunButton.Name = "rerunButton";
            rerunButton.Text = "RERUN";
            rerunButton.Font = new Font("Calibri", 12, FontStyle.Bold);
            rerunButton.BackColor = Color.Yellow;
            rerunButton.ForeColor = Color.Black;
            rerunButton.FlatStyle = FlatStyle.Flat;
            rerunButton.FlatAppearance.BorderSize = 0;
            rerunButton.Width = 70;
            rerunButton.Height = 31;
            rerunButton.Location = new System.Drawing.Point(251, 6);
            rerunButton.Click += delegate (object obj, EventArgs ea)
            {
                rerunButton.Visible = false;
                rerunScanFile(folderName, fileName, panel, true);

                string payload = "{\"type\":\"file_reran\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"threatscore\",\"message\":\"File reran\"}}";
                agentStat(payload);
            };

            Button releaseButton = new Button();
            releaseButton.Name = "releaseButton";
            releaseButton.Text = "RELEASE";
            releaseButton.Font = new Font("Calibri", 12, FontStyle.Bold);
            releaseButton.BackColor = Color.Goldenrod;
            releaseButton.ForeColor = Color.White;
            releaseButton.FlatStyle = FlatStyle.Flat;
            releaseButton.FlatAppearance.BorderSize = 0;
            releaseButton.Width = 85;
            releaseButton.Height = 31;
            releaseButton.Location = new System.Drawing.Point(414, 6);
            releaseButton.Click += delegate (object obj, EventArgs ea)
            {
                handleRelease(folderName + "\\" + fileName, false);
                string payload = "{\"type\":\"file_released\",\"payload\":{\"name\":\"" + fileName + "\",\"message\":\"File released\"}}";
                agentStat(payload);
                releaseButton.Visible = false;
            };

            // update colors based on score
            if (score_num < 20.0)
            {
                colorPanel.BackColor = Color.Green;
                percentLabel.ForeColor = Color.Green;
            }
            else if (score_num >= 20.0 && score_num < 40.0)
            {
                colorPanel.BackColor = Color.Yellow;
                percentLabel.ForeColor = Color.Yellow;
            }
            else if (score_num >= 40.0 && score_num < 60.0)
            {
                colorPanel.BackColor = Color.Orange;
                percentLabel.ForeColor = Color.Orange;
            }
            else
            {
                colorPanel.BackColor = Color.Red;
                percentLabel.ForeColor = Color.Red;
            }

            if (success)
            {
                fileLabel.Text = fileName;
                fileLabel.ForeColor = Color.White;
            }
            else
            {
                fileLabel.Text = "[FAILED] " + fileName;
                fileLabel.ForeColor = Color.Yellow;
                colorPanel.BackColor = Color.Yellow;
                panel.BackColor = Color.DarkRed;
            }

            panel.Controls.Add(colorPanel);
            panel.Controls.Add(fileLabel);
            panel.Controls.Add(folderLabel);
            panel.Controls.Add(percentLabel);
            panel.Controls.Add(rerunButton);
            if (success)
            {
                // panel.Controls.Add(percentLabel);
                rerunButton.Visible = false;
                if (score_num > this.minThreatScore)
                {
                    panel.Controls.Add(removeButton);
                    panel.Controls.Add(releaseButton);
                }
                else
                {
                    handleRelease(folderName + "\\" + fileName, false);
                }
            }
            else
            {
                // panel.Controls.Add(rerunButton);
                percentLabel.Visible = false;
                panel.Controls.Add(removeButton);
                panel.Controls.Add(releaseButton);
            }

            monitoringFlowLayoutPanel.Controls.Add(panel);
        }

        public async void rerunScanFile(string folderName, string fileName, Panel panel, bool isThreat)
        {
            try
            {
                Button rerunButton = (Button)panel.Controls.Find("rerunButton", true)[0];
                rerunButton.Visible = false;

                Label percentLabel = (Label)panel.Controls.Find("percentLabel", true)[0];
                percentLabel.Visible = true;
                percentLabel.Font = new Font("Calibri", 11, FontStyle.Regular);
                percentLabel.Location = new System.Drawing.Point(248, 14);
                percentLabel.ForeColor = Color.White;
                percentLabel.Text = "Scanning...";

                string path = folderName + "\\" + fileName;
                string hashFileName = folderName.Replace("\\", "-").Replace(":", "") + fileName + "-hash.json";

                string responseString = "";
                // save to hash file
                if (isThreat == true)
                {
                    responseString = await getThreatScore(path, fileName, "threatscore");
                    File.WriteAllText(@"./malcore/threat/" + hashFileName, responseString);

                    Label fileLabel = (Label)panel.Controls.Find("fileLabel", true)[0];
                    fileLabel.Click += delegate (object obj, EventArgs ea)
                    {
                        DetailsForm detailsForm = new DetailsForm("threat", responseString, folderName, fileName, this, panel);
                        detailsForm.Show(this);
                    };
                    Label folderLabel = (Label)panel.Controls.Find("folderLabel", true)[0];
                    folderLabel.Click += delegate (object obj, EventArgs ea)
                    {
                        DetailsForm detailsForm = new DetailsForm("threat", responseString, folderName, fileName, this, panel);
                        detailsForm.Show(this);
                    };
                }
                else
                {
                    responseString = await getThreatScore(path, fileName, "docfile");
                    File.WriteAllText(@"./malcore/doc/" + hashFileName, responseString);

                    Label fileLabel = (Label)panel.Controls.Find("fileLabel", true)[0];
                    fileLabel.Click += delegate (object obj, EventArgs ea)
                    {
                        DetailsForm detailsForm = new DetailsForm("doc", responseString, folderName, fileName, this, panel);
                        detailsForm.Show(this);
                    };
                    Label folderLabel = (Label)panel.Controls.Find("folderLabel", true)[0];
                    folderLabel.Click += delegate (object obj, EventArgs ea)
                    {
                        DetailsForm detailsForm = new DetailsForm("doc", responseString, folderName, fileName, this, panel);
                        detailsForm.Show(this);
                    };
                }

                bool success = true;

                if (responseString == "")
                {
                    // Button rerunButton = (Button)panel.Controls.Find("rerunButton", true)[0];
                    // rerunButton.Visible = true;
                    success = false;
                }
                else
                {
                    string score = "";
                    double score_num = 0;
                    JObject jsonObject = JObject.Parse(responseString);

                    success = (bool)jsonObject["success"];
                    if (jsonObject["data"]["data"].ToString() == "{}")
                    {
                        success = false;
                    }

                    if (success)
                    {
                        panel.BackColor = Color.Black;
                        if (isThreat == true)
                        {
                            score = (string)jsonObject["data"]["data"]["score"];
                            string[] scores = score.Split('/');
                            score_num = Convert.ToDouble(scores[0]);
                            score = Convert.ToString(Math.Round(score_num)) + "%";
                        }
                        else
                        {
                            score = (string)jsonObject["data"]["data"]["dfi"]["results"]["dfi_results"]["score"];
                            score_num = Convert.ToDouble(score);
                            score = Convert.ToString(Math.Round(score_num)) + "%";
                        }

                        Panel colorPanel = (Panel)panel.Controls.Find("colorPanel", true)[0];
                        Label fileLabel = (Label)panel.Controls.Find("fileLabel", true)[0];

                        percentLabel.Text = score;
                        percentLabel.Font = new Font("Calibri", 20, FontStyle.Bold);
                        percentLabel.Location = new System.Drawing.Point(252, 4);

                        fileLabel.Text = fileName;
                        fileLabel.ForeColor = Color.White;
                        // update colors based on score
                        if (score_num < 20.0)
                        {
                            colorPanel.BackColor = Color.Green;
                            percentLabel.ForeColor = Color.Green;
                        }
                        else if (score_num >= 20.0 && score_num < 40.0)
                        {
                            colorPanel.BackColor = Color.Yellow;
                            percentLabel.ForeColor = Color.Yellow;
                        }
                        else if (score_num >= 40.0 && score_num < 60.0)
                        {
                            colorPanel.BackColor = Color.Orange;
                            percentLabel.ForeColor = Color.Orange;
                        }
                        else
                        {
                            colorPanel.BackColor = Color.Red;
                            percentLabel.ForeColor = Color.Red;
                        }

                        if (score_num <= this.minThreatScore)
                        {
                            handleRelease(path, false);
                        }

                    }

                }

                if (success == false)
                {
                    percentLabel.Visible = false;
                    rerunButton.Visible = true;
                    panel.BackColor = Color.DarkRed;
                }
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine("rerunScanFile.........................." + ex);
                Button rerunButton = (Button)panel.Controls.Find("rerunButton", true)[0];
                rerunButton.Visible = true;

                Label percentLabel = (Label)panel.Controls.Find("percentLabel", true)[0];
                percentLabel.Visible = false;
                percentLabel.Font = new Font("Calibri", 20, FontStyle.Bold);
                percentLabel.Location = new System.Drawing.Point(252, 4);
                panel.BackColor = Color.DarkRed;
            }

        }

        private void addItemToMonitoringPanelForDoc(string responseString, string folderName, string fileName, bool succeed)
        {
            bool success = false;
            string score = "";
            double score_num = 0;

            if (succeed == true)
            {
                JObject jsonObject = JObject.Parse(responseString);

                success = (bool)jsonObject["success"];
                if (jsonObject["data"]["data"].ToString() == "{}")
                {
                    success = false;
                }

                if (success)
                {
                    score = (string)jsonObject["data"]["data"]["dfi"]["results"]["dfi_results"]["score"];
                    score_num = Convert.ToDouble(score);
                    score = Convert.ToString(Math.Round(score_num)) + "%";
                }
            }

            // item panel
            Panel panel = new Panel();
            panel.Width = 501;
            panel.Height = 44;
            panel.BackColor = Color.Black;

            // color panel
            Panel colorPanel = new Panel();
            colorPanel.Name = "colorPanel";
            colorPanel.Width = 20;
            colorPanel.Height = 44;
            // colorPanel.BackColor = Color.Red;

            // file label
            Label fileLabel = new Label();
            fileLabel.Name = "fileLabel";
            fileLabel.Font = new Font("Calibri", 12, FontStyle.Bold);
            fileLabel.AutoSize = false;
            fileLabel.Width = 200;
            fileLabel.Location = new System.Drawing.Point(22, 1);
            fileLabel.Click += delegate (object obj, EventArgs ea)
            {
                DetailsForm detailsForm = new DetailsForm("doc", responseString, folderName, fileName, this, panel);
                detailsForm.Show(this);
            };

            // folder label
            Label folderLabel = new Label();
            folderLabel.Name = "folderLabel";
            folderLabel.Text = folderName;
            folderLabel.ForeColor = Color.White;
            folderLabel.Font = new Font("Calibri", 11, FontStyle.Regular);
            folderLabel.AutoSize = false;
            folderLabel.Width = 220;
            // folderLabel.MaximumSize = new System.Drawing.Size(200, 0);
            folderLabel.Location = new System.Drawing.Point(24, 22);
            folderLabel.Click += delegate (object obj, EventArgs ea)
            {
                DetailsForm detailsForm = new DetailsForm("doc", responseString, folderName, fileName, this, panel);
                detailsForm.Show(this);
            };

            // percent label
            Label percentLabel = new Label();
            percentLabel.Name = "percentLabel";
            percentLabel.Text = score;
            // percentLabel.ForeColor = Color.Red;
            percentLabel.Font = new Font("Calibri", 20, FontStyle.Bold);
            percentLabel.Width = 76;
            percentLabel.Height = 40;
            percentLabel.Location = new System.Drawing.Point(252, 4);

            Button removeButton = new Button();
            removeButton.Name = "removeButton";
            removeButton.Text = "DELETE";
            removeButton.Font = new Font("Calibri", 12, FontStyle.Bold);
            removeButton.BackColor = Color.Red;
            removeButton.ForeColor = Color.White;
            removeButton.FlatStyle = FlatStyle.Flat;
            removeButton.FlatAppearance.BorderSize = 0;
            removeButton.Width = 85;
            removeButton.Height = 31;
            removeButton.Location = new System.Drawing.Point(325, 6);
            removeButton.Click += delegate (object obj, EventArgs ea)
            {
                if (File.Exists(folderName + "\\" + fileName))
                {
                    handleRelease(folderName + "\\" + fileName, false);
                    File.Delete(folderName + "\\" + fileName);
                }
                string hashFileName = folderName.Replace("\\", "-").Replace(":", "") + fileName + "-hash.json";
                if (File.Exists("./malcore/doc/" + hashFileName))
                {

                    File.Delete("./malcore/doc/" + hashFileName);
                }
                panel.Dispose();

                string payload = "{\"type\":\"file_deleted\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"docfile\",\"message\":\"File deleted\"}}";
                agentStat(payload);
            };

            Button rerunButton = new Button();
            rerunButton.Name = "rerunButton";
            rerunButton.Text = "RERUN";
            rerunButton.Font = new Font("Calibri", 12, FontStyle.Bold);
            rerunButton.BackColor = Color.Yellow;
            rerunButton.ForeColor = Color.Black;
            rerunButton.FlatStyle = FlatStyle.Flat;
            rerunButton.FlatAppearance.BorderSize = 0;
            rerunButton.Width = 70;
            rerunButton.Height = 31;
            rerunButton.Location = new System.Drawing.Point(251, 6);
            rerunButton.Click += delegate (object obj, EventArgs ea)
            {
                rerunButton.Visible = false;
                rerunScanFile(folderName, fileName, panel, false);

                string payload = "{\"type\":\"file_reran\",\"payload\":{\"name\":\"" + fileName + "\",\"type\":\"docfile\",\"message\":\"File reran\"}}";
                agentStat(payload);
            };

            Button releaseButton = new Button();
            releaseButton.Name = "releaseButton";
            releaseButton.Text = "RELEASE";
            releaseButton.Font = new Font("Calibri", 12, FontStyle.Bold);
            releaseButton.BackColor = Color.Goldenrod;
            releaseButton.ForeColor = Color.White;
            releaseButton.FlatStyle = FlatStyle.Flat;
            releaseButton.FlatAppearance.BorderSize = 0;
            releaseButton.Width = 85;
            releaseButton.Height = 31;
            releaseButton.Location = new System.Drawing.Point(414, 6);
            releaseButton.Click += delegate (object obj, EventArgs ea)
            {
                handleRelease(folderName + "\\" + fileName, false);
                string payload = "{\"type\":\"file_released\",\"payload\":{\"name\":\"" + fileName + "\",\"message\":\"File released\"}}";
                agentStat(payload);
                releaseButton.Visible = false;
            };

            // update colors based on score
            if (score_num < 20.0)
            {
                colorPanel.BackColor = Color.Green;
                percentLabel.ForeColor = Color.Green;
            }
            else if (score_num >= 20.0 && score_num < 40.0)
            {
                colorPanel.BackColor = Color.Yellow;
                percentLabel.ForeColor = Color.Yellow;
            }
            else if (score_num >= 40.0 && score_num < 60.0)
            {
                colorPanel.BackColor = Color.Orange;
                percentLabel.ForeColor = Color.Orange;
            }
            else
            {
                colorPanel.BackColor = Color.Red;
                percentLabel.ForeColor = Color.Red;
            }

            if (success)
            {
                fileLabel.Text = fileName;
                fileLabel.ForeColor = Color.White;
            }
            else
            {
                fileLabel.Text = "[FAILED] " + fileName;
                fileLabel.ForeColor = Color.Yellow;
                colorPanel.BackColor = Color.Yellow;
                panel.BackColor = Color.DarkRed;
            }

            panel.Controls.Add(colorPanel);
            panel.Controls.Add(fileLabel);
            panel.Controls.Add(folderLabel);
            panel.Controls.Add(percentLabel);
            panel.Controls.Add(rerunButton);
            if (success)
            {
                rerunButton.Visible = false;
                if (score_num > this.minThreatScore)
                {
                    panel.Controls.Add(removeButton);
                    panel.Controls.Add(releaseButton);
                }
                else
                {
                    handleRelease(folderName + "\\" + fileName, false);
                }
            }
            else
            {
                percentLabel.Visible = false;
                panel.Controls.Add(removeButton);
                panel.Controls.Add(releaseButton);
            }

            monitoringFlowLayoutPanel.Controls.Add(panel);
        }

        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        public async void ProcessFile(string path)
        {
            try
            {
                string fileName = Path.GetFileName(path);
                string folderName = Directory.GetParent(path) != null ? Directory.GetParent(path).FullName : path;

                string hashFileName = folderName.Replace("\\", "-").Replace(":", "") + fileName + "-hash.json";
                // check if the hash file is already scanned
                if (File.Exists("./malcore/threat/" + hashFileName))
                {
                    string fileString = File.ReadAllText("./malcore/threat/" + hashFileName);
                    bool succeed = true;
                    if (fileString == "")
                    {
                        succeed = false;
                    }

                    addItemToMonitoringPanel(fileString, folderName, fileName, succeed);
                }
                else if (File.Exists("./malcore/doc/" + hashFileName))
                {
                    string fileString = File.ReadAllText("./malcore/doc/" + hashFileName);
                    bool succeed = true;
                    if (fileString == "")
                    {
                        succeed = false;
                    }
                    addItemToMonitoringPanelForDoc(fileString, folderName, fileName, succeed);
                }
                else
                {
                    handleRelease(path, false);
                    // check if the file is allowed
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            FileInfo fInfo = new FileInfo(path);

                            // only allow files their size is smaller than 15M
                            if (fInfo.Length < 15728640)
                            {
                                byte[] buffer = new byte[10];
                                buffer = reader.ReadBytes(10);
                                if (buffer.Length > 9)
                                {
                                    // exe file
                                    if (buffer[0] == 77 && buffer[1] == 90)
                                    {
                                        string responseString = await getThreatScore(path, fileName, "threatscore");

                                        // save to hash file
                                        File.WriteAllText(@"./malcore/threat/" + hashFileName, responseString);

                                        bool succeed = true;
                                        if (responseString == "")
                                        {
                                            succeed = false;
                                        }

                                        // add to mornitoring list 
                                        addItemToMonitoringPanel(responseString, folderName, fileName, succeed);
                                    }
                                    else if ((buffer[0] == 37 && buffer[1] == 80 && buffer[2] == 68 && buffer[3] == 70) ||
                                    (buffer[0] == 80 && buffer[1] == 75))
                                    {
                                        string responseString = await getThreatScore(path, fileName, "docfile");

                                        // save to hash file
                                        File.WriteAllText(@"./malcore/doc/" + hashFileName, responseString);

                                        bool succeed = true;
                                        if (responseString == "")
                                        {
                                            succeed = false;
                                        }
                                        addItemToMonitoringPanelForDoc(responseString, folderName, fileName, succeed);
                                    }
                                }
                            }

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProcessFile failed----------------" + path + "-----------" + ex);
            }
        }

        private void handleRelease(string path, bool locking)
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            FileInfo fInfo = new FileInfo(path);
            FileSecurity fSecurity = fInfo.GetAccessControl();

            if (locking)
            {
                fSecurity.AddAccessRule(new FileSystemAccessRule(userName, FileSystemRights.ReadAndExecute, AccessControlType.Deny));
                fSecurity.AddAccessRule(new FileSystemAccessRule(@"SYSTEM", FileSystemRights.ReadAndExecute, AccessControlType.Deny));
                fInfo.SetAccessControl(fSecurity);
            }
            else
            {
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(userName, FileSystemRights.ReadAndExecute, AccessControlType.Deny));
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(@"SYSTEM", FileSystemRights.ReadAndExecute, AccessControlType.Deny));
                fInfo.SetAccessControl(fSecurity);
            }
        }

        private void btnLogout_Click_1(object sender, EventArgs e)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@".malcore", true);
                key.DeleteValue("API_KEY");
                key.DeleteValue("SETTINGS");
                key.Close();

                Program.APIKEY = "";
                Program.USEREMAIL = "";

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show(this);
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {
        }

        private void labelInactiveDescription_Click(object sender, EventArgs e)
        {

        }

        private void btnSettings_Click_1(object sender, EventArgs e)
        {
            Hide();
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.Show(this);
        }

        private void fileSystemWatcherMain_Created_1(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                ProcessFile(e.FullPath);
            }
        }

        private void fileSystemWatcherMain_Changed(object sender, FileSystemEventArgs e)
        {
        }

        private void MonitoringForm_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                // this is for notification
                // notifyIcon1.ShowBalloonTip(500);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void MonitoringForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.closing == false)
            {
                e.Cancel = true;
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.closing = true;
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            Application.Exit();
        }
    }
}
