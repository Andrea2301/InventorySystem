using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using InventorySystem.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class CheckoutViewModel : ViewModelBase, ICloseableViewModel
    {
        private readonly IMessageService _messageService;
        private decimal _totalAmount;
        private Client? _selectedClient;
        private string _paymentMethod = "Efectivo"; // Default
        private decimal _cashReceived;
        private decimal _changeDue;
        private string _paymentStatus = "Pendiente";
        private bool _isPaymentSuccessful;
        private bool _isProcessingPayment;
        private string _selectedCurrency = "COP";
        private string[] _currencies = new[] { "COP", "USD", "EUR" };

        public event EventHandler? RequestClose;

        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(nameof(TotalAmount)); }
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(nameof(SelectedClient)); }
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
                ResetPaymentState();
                OnPropertyChanged(nameof(IsCashPayment));
                OnPropertyChanged(nameof(IsDigitalPayment));
            }
        }

        public bool IsCashPayment
        {
            get => PaymentMethod == "Efectivo";
            set
            {
                if (value)
                {
                    PaymentMethod = "Efectivo";
                }
            }
        }

        public bool IsDigitalPayment
        {
            get => PaymentMethod == "Pago Digital";
            set
            {
                if (value)
                {
                    PaymentMethod = "Pago Digital";
                }
            }
        }

        public decimal CashReceived
        {
            get => _cashReceived;
            set
            {
                _cashReceived = value;
                OnPropertyChanged(nameof(CashReceived));
                CalculateChange();
            }
        }

        public decimal ChangeDue
        {
            get => _changeDue;
            set { _changeDue = value; OnPropertyChanged(nameof(ChangeDue)); }
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set { _paymentStatus = value; OnPropertyChanged(nameof(PaymentStatus)); }
        }

        public bool IsPaymentSuccessful
        {
            get => _isPaymentSuccessful;
            set { _isPaymentSuccessful = value; OnPropertyChanged(nameof(IsPaymentSuccessful)); }
        }

        public bool IsProcessingPayment
        {
            get => _isProcessingPayment;
            set { _isProcessingPayment = value; OnPropertyChanged(nameof(IsProcessingPayment)); }
        }

        public string[] Currencies => _currencies;

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged(nameof(SelectedCurrency));
                OnPropertyChanged(nameof(CurrencySymbol));
                OnPropertyChanged(nameof(QrCodeUrl));
            }
        }

        public string CurrencySymbol
        {
            get
            {
                return SelectedCurrency switch
                {
                    "USD" => "$",
                    "EUR" => "€",
                    "COP" => "$",
                    _ => "$"
                };
            }
        }

        public string QrCodeUrl
        {
            get
            {
                // Genera una URL scaneable con el servicio de QR Server que apunta a un checkout simulado de Stripe.
                string stripeCheckoutSimUrl = $"https://checkout.stripe.com/pay/cs_test_simulated_sale_{Guid.NewGuid()}?amount={TotalAmount}&currency={SelectedCurrency.ToLower()}";
                return $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={Uri.EscapeDataString(stripeCheckoutSimUrl)}";
            }
        }

        public ICommand ConfirmPaymentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SimulatePaymentCommand { get; }

        public CheckoutViewModel(decimal totalAmount, Client? selectedClient, IMessageService messageService)
        {
            _totalAmount = totalAmount;
            _selectedClient = selectedClient;
            _messageService = messageService;

            ConfirmPaymentCommand = new ViewModelCommand(ExecuteConfirmPayment, CanConfirmPayment);
            CancelCommand = new ViewModelCommand(ExecuteCancel);
            SimulatePaymentCommand = new ViewModelCommand(ExecuteSimulatePayment, CanSimulatePayment);

            CalculateChange();
        }

        private void CalculateChange()
        {
            if (CashReceived >= TotalAmount)
            {
                ChangeDue = CashReceived - TotalAmount;
            }
            else
            {
                ChangeDue = 0;
            }
        }

        private void ResetPaymentState()
        {
            if (PaymentMethod == "Efectivo")
            {
                PaymentStatus = "Pendiente";
                CashReceived = 0;
                ChangeDue = 0;
            }
            else
            {
                PaymentStatus = "Pendiente (Esperando pago)";
            }
        }

        private bool CanConfirmPayment(object obj)
        {
            if (PaymentMethod == "Efectivo")
            {
                return CashReceived >= TotalAmount;
            }
            else // Pago Digital
            {
                return PaymentStatus == "Aprobado";
            }
        }

        private void ExecuteConfirmPayment(object obj)
        {
            IsPaymentSuccessful = true;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteCancel(object obj)
        {
            IsPaymentSuccessful = false;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private bool CanSimulatePayment(object obj)
        {
            return !IsProcessingPayment && PaymentStatus != "Aprobado";
        }

        private async void ExecuteSimulatePayment(object obj)
        {
            IsProcessingPayment = true;
            PaymentStatus = "Verificando con pasarela Stripe...";

            try
            {
                // Simula el tiempo de conexión y validación de la transacción
                await Task.Delay(2000);
                PaymentStatus = "Aprobado (Pago verificado)";
                _messageService.ShowInfo("Pago procesado exitosamente por la pasarela de Stripe.");
            }
            catch (Exception ex)
            {
                PaymentStatus = "Error en el pago";
                _messageService.ShowError($"Error al verificar el pago: {ex.Message}");
            }
            finally
            {
                IsProcessingPayment = false;
                // Forzar reevaluación del botón confirmar
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
