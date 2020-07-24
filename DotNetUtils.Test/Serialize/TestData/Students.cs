using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Test.Serialize.TestData
{
    public partial class Students
    {

        private StudentsStudent[] studentField;

        public StudentsStudent[] Student
        {
            get
            {
                return this.studentField;
            }
            set
            {
                this.studentField = value;
            }
        }
    }

    public partial class StudentsStudent
    {

        private byte rollNoField;

        private string nameField;

        private string addressField;

        /// <remarks/>
        public byte RollNo
        {
            get
            {
                return this.rollNoField;
            }
            set
            {
                this.rollNoField = value;
            }
        }

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }
    }
}
