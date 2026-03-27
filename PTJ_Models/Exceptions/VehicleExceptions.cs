using System;

namespace Models.Exceptions
{
    public class VehicleBaseException : Exception
    {
        public string ErrorCode { get; }
        public string Field { get; }
        public string Severity { get; }

        public VehicleBaseException(string errorCode, string field, string message, string severity = "CRITICAL") 
            : base(message)
        {
            ErrorCode = errorCode;
            Field = field;
            Severity = severity;
        }
    }

    public class InvalidLicensePlateException : VehicleBaseException
    {
        public InvalidLicensePlateException(string message = "Biển số xe không đúng định dạng (Ví dụ: 29A-123.45)") 
            : base("VEHICLE_001", "licensePlate", message) { }
    }

    public class InvalidVINLengthException : VehicleBaseException
    {
        public InvalidVINLengthException(string message = "Số VIN phải có độ dài chính xác là 17 ký tự") 
            : base("VEHICLE_002", "vin", message) { }
    }

    public class ForbiddenCharacterException : VehicleBaseException
    {
        public ForbiddenCharacterException(string message = "Số VIN không được chứa các ký tự I, O, Q") 
            : base("VEHICLE_003", "vin", message) { }
    }

    public class LegalComplianceException : VehicleBaseException
    {
        public LegalComplianceException(string message = "Thiếu thiết bị bắt buộc theo Nghị định 10 (IMEI GSHT/Camera)") 
            : base("VEHICLE_004", "legal", message) { }
    }

    public class PastDateException : VehicleBaseException
    {
        public PastDateException(string field, string message = "Ngày hết hạn không thể ở quá khứ") 
            : base("VEHICLE_005", field, message) { }
    }

    public class DuplicateVehicleException : VehicleBaseException
    {
        public DuplicateVehicleException(string field, string value) 
            : base("VEHICLE_006", field, $"Giá trị '{value}' đã tồn tại trong hệ thống") { }
    }
}
