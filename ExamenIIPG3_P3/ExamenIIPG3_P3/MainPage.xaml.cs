using Acr.UserDialogs;
using ExamenIIPG3_P3.Controlador;
using ExamenIIPG3_P3.Modelo;
using ExamenIIPG3_P3.Vista;
using Plugin.AudioRecorder;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExamenIIPG3_P3
{
    public partial class MainPage : ContentPage
    {

        byte[] Image;
        private AudioRecorderService audioRecorderService = new AudioRecorderService()
        {
            StopRecordingOnSilence = false,
            StopRecordingAfterTimeout = false
        };

        private AudioPlayer audioPlayer = new AudioPlayer();

        private bool reproducir = false;
        MediaFile FileFoto = null;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            getLatitudeAndLongitude();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            bool response = await Application.Current.MainPage.DisplayAlert("Advertencia", "Escoja como o de donde quiere que provenga su imagen", "Camara", "Galeria");

            if (response)
                GetImageFromCamera();
            else
                GetImageFromGallery();
        }

        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Microphone>();
                var status2 = await Permissions.RequestAsync<Permissions.StorageRead>();
                var status3 = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted & status2 != PermissionStatus.Granted & status3 != PermissionStatus.Granted)
                {
                    return; // si no tiene los permisos no avanza
                }

                if (audioRecorderService.IsRecording)
                {
                    await audioRecorderService.StopRecording();


                    audioPlayer.Play(audioRecorderService.GetAudioFilePath());

                    //txtMessage.Text = "No esta grabando";

                    //txtMessage.TextColor = Color.Red;

                    btnGrabar.Text = "Grabar";

                    reproducir = true;
                }
                else
                {
                    await audioRecorderService.StartRecording();


                   // txtMessage.Text = "Esta grabando";

                    //txtMessage.TextColor = Color.Green;

                    btnGrabar.Text = "Parar";

                    //reproducir = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alerta", ex.Message, "OK");
            }

        }

        private async void btnAdd_Clicked(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current != NetworkAccess.Internet)
            {
                Message("Advertencia", "No cuenta con acceso a internet, conectese a algun proveedor mas cercano");
                return;
            }

            if (Image == null)
            {
                Message("Aviso", "Presione la imagen para tomar el video, es un requisito");
                return;
            }

            if (string.IsNullOrEmpty(txtLatitude.Text) || string.IsNullOrEmpty(txtLongitude.Text))
            {
                Message("Aviso", "Upps, aun no obtenemos la ubicacion");
                getLatitudeAndLongitude();
                return;
            }

            if (string.IsNullOrEmpty(txtDescription.Text))
            {
                Message("Aviso", "Describa el sitio donde se encuentre");
                return;
            }

            if (txtDescription.Text.Length > 100)
            {
                Message("Aviso", "Su descripción no debe de tener mas de 100 caracteres");

                return;
            }

            if (!reproducir)
            {
                Message("Aviso", "Grabe un audio cualquiera, es un requisito");
                return;
            }


            var length = ConvertAudioToByteArray().Length;

            if (length > 1500000)
            {
                Message("Aviso", "El audio debe ser mas corto");
                return;
            }


            try
            {

                UserDialogs.Instance.ShowLoading("Guardando del sitio en proceso", MaskType.Clear);

                var sitio = new Sitio()
                {
                    Latitude = double.Parse(txtLatitude.Text),
                    Longitude = double.Parse(txtLongitude.Text),
                    Description = txtDescription.Text,
                    Image = Image,
                    AudioFile = ConvertAudioToByteArray()
                    //pathImage = FileFoto.Path
                };

                var result = await ControladorSitio.CreateSite(sitio);

                UserDialogs.Instance.HideLoading();
                await Task.Delay(500);

                if (result)
                {
                    Message("Aviso", "El sitio se guardo de manera integra");
                    clearComp();
                }
                else
                {
                    Message("Aviso", "Lo sentimos, no se pudo agregar el sitio");
                }

            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();

                await Task.Delay(500);

                Message("Error: ", ex.Message);
            }
        }

        private async void btnList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new Lista());
        }


        private async void GetImageFromGallery()
        {
            try
            {
                if (CrossMedia.Current.IsPickPhotoSupported)
                {
                    var FileFoto = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium,
                    });
                    if (FileFoto == null)
                        return;

                    imgFoto.Source = ImageSource.FromStream(() => { return FileFoto.GetStream(); });
                    Image = File.ReadAllBytes(FileFoto.Path);
                }
                else
                {
                    Message("Advertencia", "Error al seleccionar el video");
                }
            }
            catch (Exception)
            {
                Message("Advertencia", "Error al seleccionar el video");
            }

        }

        private async void GetImageFromCamera()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status == PermissionStatus.Granted)
            {
                try
                {
                    FileFoto = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium,
                        SaveToAlbum = true
                    });

                    if (FileFoto == null)
                        return;

                    imgFoto.Source = ImageSource.FromStream(() => { return FileFoto.GetStream(); });
                    Image = File.ReadAllBytes(FileFoto.Path);
                }
                catch (Exception)
                {
                    Message("Advertencia", "Error al tomar el video");
                }
            }
            else
            {
                await Permissions.RequestAsync<Permissions.Camera>();
            }
        }

        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        private async void getLatitudeAndLongitude()
        {
            try
            {

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var localizacion = await Geolocation.GetLocationAsync();
                    txtLatitude.Text = Math.Round(localizacion.Latitude, 5) + "";
                    txtLongitude.Text = Math.Round(localizacion.Longitude, 5) + "";
                }
                else
                {
                    await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
            }
            catch (Exception e)
            {

                if (e.Message.Equals("Location services are not enabled on device."))
                {

                    Message("Error", "Servicio de localizacion apagado");
                }
                else
                {
                    Message("Error", e.Message);
                }

            }
        }

        private void clearComp()
        {
            imgFoto.Source = "imgMuestra.png";
            txtDescription.Text = "";
            Image = null;
            getLatitudeAndLongitude();
        }

        private Byte[] ConvertAudioToByteArray()
        {
            Stream audioFile = audioRecorderService.GetAudioFileStream();

            //var mStream = new MemoryStream(File.ReadAllBytes(audioRecorderService.GetAudioFilePath()));
            //var mStream = (MemoryStream)audioFile;

            Byte[] bytes = ReadFully(audioFile);
            return bytes;
        }

        private Byte[] ConvertImageToByteArray()
        {
            if (FileFoto != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Stream stream = FileFoto.GetStream();

                    stream.CopyTo(memory);

                    return memory.ToArray();
                }
            }

            return null;
        }

        private async void Message(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }

    }
}
