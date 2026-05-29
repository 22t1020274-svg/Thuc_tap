using SV22T1020274.Domain.HR;


namespace SV22T1020274.Admin.Models
{
    public class EmployeeRoleViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}

