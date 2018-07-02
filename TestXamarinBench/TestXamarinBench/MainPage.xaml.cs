using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PhoneNumbers;
using Xamarin.Forms;

namespace TestXamarinBench
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private RegionInfo _country;
        private PhoneNumberUtil _phoneNumberUtil;
        private string _countryName = string.Empty;

        public string CountryName
        {
            get => _countryName;
            set => SetField(ref _countryName, value, nameof(CountryName));
        }

        private string _phoneNumberStatusText = string.Empty;

        public string PhoneNumberStatusText
        {
            get => _phoneNumberStatusText;
            set => SetField(ref _phoneNumberStatusText, value, nameof(PhoneNumberStatusText));
        }

        private string _phoneNumberFailResonText = string.Empty;

        public string PhoneNumberFailResonText
        {
            get => _phoneNumberFailResonText;
            set => SetField(ref _phoneNumberFailResonText, value, nameof(PhoneNumberFailResonText));
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            Initialize();
            PhoneNumberEntry.TextChanged += HandlePhoneNumberTextChanges;
        }

        private void Initialize()
        {
            _phoneNumberUtil = PhoneNumberUtil.GetInstance();
            _country = GetCountry();
            if (_country == null)
                return;

            CountryName = _country.EnglishName;
        }

        private RegionInfo GetCountry()
        {
            return RegionInfo.CurrentRegion;
        }

        private void HandlePhoneNumberTextChanges(object sender, TextChangedEventArgs e)
        {
            Task.Run(async () =>
            {
                bool valid = false;
                bool validForRegion = true;
                string invalidPhoneNumberReason = "Phone Number is valid";
                string regionValidity = string.Empty;

                try
                {
                    var phoneNumberObject = GetPhoneNumberObject(e.NewTextValue);
                    valid = ValidatePhoneNumber(phoneNumberObject);

                    if (!valid)
                    {
                        invalidPhoneNumberReason = GetInvalidPhoneNumberReason(phoneNumberObject);
                    }
                    else
                    {
                        validForRegion = ValidatePhoneNumberAgainstRegion(phoneNumberObject);
                        if (!validForRegion)
                            regionValidity = $"Not valid for {_countryName}, Possibly valid for {PossiblePhoneNumberRegion(phoneNumberObject)}." ;
                    }
                }
                catch (NumberParseException exception)
                {
                    valid = false;
                    if (string.IsNullOrEmpty(e.NewTextValue))
                        invalidPhoneNumberReason = "Phone Number is empty";
                    else
                        invalidPhoneNumberReason = exception.ErrorType.ToString();
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (valid)
                    {
                        PhoneNumberStatusText = "Phone number is valid";
                        if (!validForRegion)
                            PhoneNumberFailResonText = regionValidity;
                        else
                            PhoneNumberFailResonText = string.Empty;
                    }
                    else
                    {
                        PhoneNumberStatusText = "Phone number is invalid";
                        PhoneNumberFailResonText = invalidPhoneNumberReason;
                    }
                });
            });
        }

        private PhoneNumber GetPhoneNumberObject(string phoneNumberString)
        {
            return _phoneNumberUtil.Parse($"+{phoneNumberString}", null);
        }

        private bool ValidatePhoneNumber(PhoneNumber phoneNumber)
        {
            return _phoneNumberUtil.IsValidNumber(phoneNumber);
        }

        private bool ValidatePhoneNumberAgainstRegion(PhoneNumber phoneNumber)
        {
            return _phoneNumberUtil.IsValidNumberForRegion(phoneNumber, _country.Name);
        }

        private string GetInvalidPhoneNumberReason(PhoneNumber phoneNumber)
        {
            var result = _phoneNumberUtil.IsPossibleNumberWithReason(phoneNumber);
            return result.ToString();
        }

        private string PossiblePhoneNumberRegion (PhoneNumber phoneNumber)
        {
            return _phoneNumberUtil.GetRegionCodeForNumber(phoneNumber);
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}