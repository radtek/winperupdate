﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProcessMsg.Model;
using Newtonsoft.Json;
using System.Diagnostics;

namespace WinPerUpdateUI
{
    public partial class FormPrincipal : Form
    {
        const int SIZEBUFFER = 16384;
        ContextMenu ContextMenu1 = new ContextMenu();
        private bool ServerInAccept = true;
        private bool TreePoblado = false;
        private ClienteBo cliente = new ClienteBo();
        private List<AmbienteBo> ambientes = new List<AmbienteBo>();

        public FormPrincipal()
        {
            InitializeComponent();
            ServerInAccept = false;
            this.CenterToScreen();
        }

        private void Salir_Click(object sender, System.EventArgs e)
        {
            //' Este procedimiento se usa para cerrar el formulario,
            //' se usará como procedimiento de eventos, en principio usado por el botón Salir
            //this.Close();
            Application.Exit();
        }

        private void Restaurar_Click(object sender, System.EventArgs e)
        {
            //' Restaurar por si se minimizó
            //' Este evento manejará tanto los menús Restaurar como el NotifyIcon.DoubleClick
            //ShowInTaskbar = true;
            //WindowState = FormWindowState.Normal;
            var form = new frmVersiones();
            form.Show();
        }

        private void AcercaDe_Click(object sender, System.EventArgs e)
        {
            var form = new AboutWinperUpdate();
            form.Show();
        }

        private void FormPrincipal_Resize(object sender, EventArgs e)
        {
            //' Cuando se minimice, ocultarla, se quedará disponible en la barra de tareas
            if (this.WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void FormPrincipal_Activated(object sender, EventArgs e)
        {
        }

        private void notifyIcon2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //ShowInTaskbar = true;
            //WindowState = FormWindowState.Normal;
            var form = new frmVersiones();
            form.Show();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            ContextMenu1.MenuItems.Add("&Restaurar", new EventHandler(this.Restaurar_Click));
            ContextMenu1.MenuItems[0].Enabled = false;

            ContextMenu1.MenuItems.Add("Configurar Ambiente y Licencia", new EventHandler(this.Ambiente_Click));
            ContextMenu1.MenuItems[1].Enabled = false;

            ContextMenu1.MenuItems.Add("-");
            ContextMenu1.MenuItems.Add("&Acerca de...", new EventHandler(this.AcercaDe_Click));
            ContextMenu1.MenuItems[2].DefaultItem = true;

            ContextMenu1.MenuItems.Add("-");
            ContextMenu1.MenuItems.Add("&Salir", new EventHandler(this.Salir_Click));

            notifyIcon2.ContextMenu = ContextMenu1;

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;            
            timer1.Stop();

            string nroLicencia = "";
            string ambientecfg = "";

            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\WinperUpdate");
                nroLicencia = key.GetValue("Licencia").ToString();
                ambientecfg = key.GetValue("Ambientes").ToString();
                key.Close();
            }
            catch (Exception ex)
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\WinperUpdate");
                key.SetValue("Licencia", "");
                key.SetValue("Ambientes", "");
                key.Close();
            }

            if (!string.IsNullOrEmpty(nroLicencia))
            {
                string server = ConfigurationManager.AppSettings["server"];
                string port = ConfigurationManager.AppSettings["port"];

                try
                {
                    string json = Utils.StrSendMsg(server, int.Parse(port), "checklicencia#" + nroLicencia + "#");
                    cliente = JsonConvert.DeserializeObject<ClienteBo>(json);
                    if (cliente != null)
                    {
                        json = Utils.StrSendMsg(server, int.Parse(port), "ambientes#" + cliente.Id.ToString() + "#");
                        foreach (var ambiente in JsonConvert.DeserializeObject<List<AmbienteBo>>(json))
                        {
                            if (ambientecfg.Contains(ambiente.Nombre))
                            {
                                ambientes.Add(ambiente);
                            }
                        }

                        ContextMenu1.MenuItems[0].Enabled = true;
                        ContextMenu1.MenuItems[1].Enabled = true;
                        ContextMenu1.MenuItems[2].DefaultItem = false;
                        ContextMenu1.MenuItems[0].DefaultItem = true;
                        timer1.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Winper Update no tiene conexión con el servidor central");
                }
            }
            
        }

        private void Ambiente_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            var form = new Ambiente();
            form.Show();
        }

        private void FormPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            WindowState = FormWindowState.Minimized;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ServerInAccept)
            {
                // TODO: Insert monitoring activities here.

                string server = ConfigurationManager.AppSettings["server"];
                string port = ConfigurationManager.AppSettings["port"];
                string dirTmp = Path.GetTempPath();
                dirTmp += dirTmp.EndsWith("\\") ? "" : "\\";

                foreach (var item in ambientes)
                {
                    var versiones = new List<VersionBo>();
                    string json = Utils.StrSendMsg(server, int.Parse(port), "getversiones#" + cliente.Id.ToString() + "#" + item.idAmbientes.ToString() + "#");
                    versiones = JsonConvert.DeserializeObject<List<VersionBo>>(json);
                    if (versiones != null)
                    {
                        var release = versiones.SingleOrDefault(x => x.Estado == 'P');
                        if (release != null)
                        {
                            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\WinperUpdate");
                            string nroVersion = key.GetValue("Version").ToString();
                            if (nroVersion.Equals(release.Release)) return;

                            if (!File.Exists(dirTmp + release.Instalador))
                            {
                                ServerInAccept = false;

                                FileStream stream = new FileStream(dirTmp + release.Instalador, FileMode.CreateNew, FileAccess.Write);
                                BinaryWriter writer = new BinaryWriter(stream);

                                int nPosIni = 0;
                                while (nPosIni < release.Length)
                                {
                                    long largoMax = release.Length - nPosIni;
                                    if (largoMax > SIZEBUFFER) largoMax = SIZEBUFFER;
                                    string newmsg = string.Format("getfile#{0}\\Output\\{1}#{2}#{3}#", release.Release, release.Instalador, nPosIni, largoMax);
                                    var buffer = Utils.SendMsg(server, int.Parse(port), newmsg);
                                    writer.Write(buffer, 0, buffer.Length);

                                    nPosIni += SIZEBUFFER;
                                }

                                writer.Close();
                                stream.Close();

                                // Avisamos llegada de nueva versión
                                notifyIcon2.BalloonTipIcon = ToolTipIcon.Info;
                                notifyIcon2.BalloonTipText = "Existen una nueva versión de winper";
                                notifyIcon2.BalloonTipTitle = "Winper Update";

                                notifyIcon2.ShowBalloonTip(1000);

                                key.SetValue("Version", release.Release);
                                key.SetValue("Status", "");

                                ServerInAccept = true;
                                TreePoblado = true;
                            }
                            else
                            {
                                if (!nroVersion.Equals(release.Release))
                                {
                                    // Actualizamos la versión en la registry
                                    key.SetValue("Version", release.Release);
                                    key.SetValue("Status", "");
                                }
                            }

                            key.Close();
                        }
                    }
                }
            }
        }


    }
}
