namespace Models.DTO.Vehicles
{
    public class StartTripRequestDto
    {
        public int TransferPlanId { get; set; }

        public int OperatorId { get; set; }

        /// <summary>Required if departure is late vs planned date.</summary>
        public string? Note { get; set; }
    }
}
