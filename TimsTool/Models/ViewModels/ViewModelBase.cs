using Identity;
using DataLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Models.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes 
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected DataAudit data;
        protected ITreeModel treeModel;

        #region Data Auditting

        /// <summary>
        /// Fire property changed to ensure that the UI is refreshed
        /// </summary>
        public virtual DateTime? LastModifiedDate
        {
            get => data == null ? null : data.LastModifiedDate;
            set
            {
                if (SetProperty(() => data.LastModifiedDate == value, () => data.LastModifiedDate = value))
                {
                    LastModifiedUser = IdentityHelper.SignedInUser.Email;
                }
            }
        }

        /// <summary>
        /// Deleted dates are set by delete commands rather than UI actions
        /// </summary>
        public virtual DateTime? DeletedDate
        {
            get => data == null ? null : data.DeletedDate;
            set
            {
                if (SetProperty(() => data != null && data.DeletedDate == value, () => data.DeletedDate = value))
                {
                    OnPropertyChanged("IsDeleted");
                    OnPropertyChanged("IsReadOnly");
                    OnPropertyChanged("IsEnabled");

                    if (value == null) { DeletedUser = null; }
                    else { DeletedUser = IdentityHelper.SignedInUser.Email; }
                }
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public Guid? UUID
        {
            get => data == null ? null : data.UUID;
        }

        /// <summary>
        /// For drafting.  Master UUID will extend across all versions of a data object
        /// i.e. the current published version, the historical revisions and also any draft versions
        /// </summary>
        public Guid? MasterUUID
        {
            get
            {
                if (data == null) { return null; }
                if (data.MasterUUID == null)
                {
                    return UUID;
                }
                return data.MasterUUID;
            }
            set
            {
                data.MasterUUID = value;
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public DateTime? CreatedDate
        {
            get => data == null ? new DateTime?() : data.CreatedDate;
            private set
            {
                if (SetProperty(() => data.CreatedDate == value && value != null, () => data.CreatedDate = value.Value))
                {
                    CreatedUser = IdentityHelper.SignedInUser.Email;
                }
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public virtual bool IsDeleted
        {
            get => DeletedDate != null;
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public string CreatedUser
        {
            get => data == null ? null : data.CreatedUser;
            private set
            {
                SetProperty(() => data.CreatedUser == value, () => data.CreatedUser = value);
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public string LastModifiedUser
        {
            get => data == null ? null : data.LastModifiedUser;
            private set 
            {
                SetProperty(() => data.LastModifiedUser == value, () => data.LastModifiedUser = value);
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public string DeletedUser
        {
            get => data == null ? null : data.DeletedUser;
            protected set
            {
                SetProperty(() => data.DeletedUser == value, () => data.DeletedUser = value);
            }
        }

        #endregion Data Auditting

        #region Data Publishing

        /// <summary>
        /// For One way rendering
        /// </summary>
        public DateTime PublishedStatusDate
        {
            get => data.PublishedStatusDate == null ? data.LastModifiedDate == null ? data.CreatedDate : data.LastModifiedDate.Value : data.PublishedStatusDate.Value;
            private set
            {
                SetProperty(() => data.PublishedStatusDate == value, () => data.PublishedStatusDate = value);
            }
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public DateTime? OriginalPublishedDate
        {
            get => PublishedStatus == PublishedStatus.Published ? PublishedStatusDate : data.OriginalPublishedDate;
            private set
            {
                SetProperty(() => data.OriginalPublishedDate == value, () => data.OriginalPublishedDate = value);
            }
        }

        public bool IsRevision
        {
            get => PublishedStatus == PublishedStatus.Revision;
        }

        public bool IsRevisionPending
        {
            get => PublishedStatus == PublishedStatus.RevisionPending;
        }

        public virtual bool IsPublishedPending
        {
            get => PublishedStatus == PublishedStatus.PublishedPending;
        }

        /// <summary>
        /// For One way rendering
        /// </summary>
        public virtual PublishedStatus PublishedStatus
        {
            get
            {
                return data.CalculatedPublishedStatus;
            }
            set
            {
                if(SetProperty(() => data.PublishedStatus == value, () => data.PublishedStatus = value))
                {
                    //when set as revision pending (from published), preserve the original published date
                    if (value == PublishedStatus.RevisionPending)
                    {
                        OriginalPublishedDate = PublishedStatusDate;
                    }

                    PublishedStatusDate = DateTime.Now;
                    OnPropertyChanged("IsReadOnly");
                    OnPropertyChanged("IsEnabled");
                    OnPropertyChanged("IsRevision");
                    OnPropertyChanged("IsRevisionPending");
                    OnPropertyChanged("IsPublishedPending");
                }
            }
        }

        public DateTime? PublishedStatusDateRaw
        {
            get => data.PublishedStatusDate;
        }

        public PublishedStatus? PublishedStatusRaw
        {
            get => data.PublishedStatus;
        }

        private bool? isReadOnly;
        /// <summary>
        /// For One way rendering of text boxes
        /// </summary>        
        public virtual bool IsReadOnly
        {
            get => isReadOnly != null ? isReadOnly.Value : PublishedStatus == DataLib.PublishedStatus.Draft && DeletedDate == null ? false : true;
            set
            {
                if (SetProperty(() => isReadOnly == value, () => isReadOnly = value))
                {
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        private List<string> publicationTags;
        public List<string> PublicationTags
        {
            get => data == null ? publicationTags : data.PublicationTags;
            set
            {
                if (data == null)
                {
                    publicationTags = value;
                }
                else
                {
                    data.PublicationTags = value;
                }
            }
        }

        /// <summary>
        /// For One way rendering of check boxes etc
        /// </summary>
        public virtual bool IsEnabled
        {
            get => !IsReadOnly;
        }

        #endregion

        #region Ensure model is bindable

        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Notifies listeners that a property value has changed.  The C#6 version of the common implementation
        /// </summary>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers
        ///     that support <see cref="CallerMemberNameAttribute" />.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members

        /// <summary>
        ///     Checks if a property already matches a desired value.  Sets the property and
        ///     notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        /// <returns>
        ///     True if the value was changed, false if the existing value matched the
        ///     desired value.
        /// </returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]string name = null)
        {
            bool propertyChanged = false;

            //If we have a different value, do stuff
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
                propertyChanged = true;
            }

            return propertyChanged;
        }


        /// <summary>
        /// Sets the value of the property to the specified value if it has changed.
        /// </summary>
        /// <param name="equal">A function which returns <c>true</c> if the property value has changed, otherwise <c>false</c>.</param>
        /// <param name="action">The action where the property is set.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if the property was changed, otherwise <c>false</c>.</returns>
        protected bool SetProperty(
            Func<bool> equal,
            Action action,
            [CallerMemberName] string propertyName = null)
        {
            if (equal())
            {
                return false;
            }

            action();
            this.OnPropertyChanged(propertyName);

            return true;
        }


        #endregion // Ensure model is bindable

    }
}
