﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UVemyCliente.Utilidades
{
    /// <summary>
    /// Lógica de interacción para ExitoMensaje.xaml
    /// </summary>
    public partial class ExitoMensaje : Window
    {
        public ExitoMensaje()
        {
            InitializeComponent();
        }

        public ExitoMensaje(string mensaje)
        {
            InitializeComponent();
            txtBlockMensaje.Text = mensaje;
        }

        private void ClicAceptar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
