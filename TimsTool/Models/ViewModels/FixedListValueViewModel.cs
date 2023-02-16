using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Models.ViewModels
{
    public class FixedListValueViewModel : ViewModelBase
    {
        readonly FixedListViewModel fixedList;

        public FixedListValueViewModel(FixedListValue fixedListValue, FixedListViewModel fixedList)
        {
            this.FixedListValue = fixedListValue;
            this.fixedList = fixedList;
        }

        public FixedListValue FixedListValue
        {
            get { return data as FixedListValue; }
            private set { data = value; }
        }

        public string Code
        {
            get => FixedListValue.Code;
            set
                {
                    if (SetProperty(() => FixedListValue.Code == value, () => FixedListValue.Code = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string Value
        {
            get => FixedListValue.Value;
            set
                {
                    if (SetProperty(() => FixedListValue.Value == value, () => FixedListValue.Value = value))
                    {
                        LastModifiedDate = DateTime.Now;

                        //null out the welsh translation since this item is now required to be retranslated
                        WelshValue = null;
                    }
                }
        }

        public string WelshValue
        {
            get => FixedListValue.WelshValue;
            set
                {
                    if (SetProperty(() => FixedListValue.WelshValue == value, () => FixedListValue.WelshValue = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public string CJSQualifier
        {
            get => FixedListValue.CJSQualifier;
            set
                {
                    if (SetProperty(() => FixedListValue.CJSQualifier == value, () => FixedListValue.CJSQualifier = value))
                    {
                        LastModifiedDate = DateTime.Now;
                    }
                }
        }

        public override bool IsReadOnly 
        {
            get => fixedList.IsReadOnly;
        }

        /// <summary>
        /// Fire property changed to ensure that the UI is refreshed
        /// </summary>
        public override DateTime? LastModifiedDate
        {
            get => base.LastModifiedDate;
            set
            {
                base.LastModifiedDate = value;

                fixedList.LastModifiedDate = value;
            }
        }
    }
}