﻿using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LiveFootball.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string propName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    #endregion

    #region INotifyDataErrorInfo

    private readonly Dictionary<string, string> _errors = new();
    private readonly object _errorDictionaryLock = new();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public virtual void SetPropertyError(string error, [CallerMemberName] string propertyName = "")
    {
        lock (_errorDictionaryLock)
        {
            if (_errors.ContainsKey(propertyName))
                _errors[propertyName] = error;
            else
                _errors.Add(propertyName, error);
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public virtual void RemovePropertyError([CallerMemberName] string propertyName = "")
    {
        lock (_errorDictionaryLock)
        {
            if (_errors.Remove(propertyName))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    public void RemoveAllPropertiesErrors()
    {
        List<string> propertyNames = null;
        lock (_errorDictionaryLock)
        {
            if (_errors.Any())
            {
                propertyNames = new List<string>(_errors.Keys);
                _errors.Clear();
            }
        }

        if (propertyNames == null) return;

        foreach (var propertyName in propertyNames)
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public virtual IEnumerable GetErrors(string propertyName)
    {
        string errorsForName;

        if (propertyName == null)
            return null;
        lock (_errorDictionaryLock)
        {
            _errors.TryGetValue(propertyName, out errorsForName);
        }

        if (errorsForName != null)
            return new List<string> { errorsForName };

        return null;
    }

    public IEnumerable<string> GetAllErrorMessages()
    {
        lock (_errorDictionaryLock)
        {
            return _errors?.Values;
        }
    }

    public virtual bool HasErrors
    {
        get
        {
            bool hasError;
            lock (_errorDictionaryLock)
            {
                hasError = _errors.Any();
            }

            return hasError;
        }
    }

    #endregion

    #region Validators

    /// <summary>
    ///     Sets a storage value (generally a backing variable for a property) to a new value and, if called from a property's
    ///     setter, raises
    ///     the PropertyChanged event on that property.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing variable for the calling property to be updated passed by reference.</param>
    /// <param name="value">The new value passed to the setter.</param>
    /// <param name="propertyName">The calling property name. Automatically deducted if null.</param>
    /// <example>
    ///     set
    ///     {
    ///     // This is the setter for a property.
    ///     if (!SetProperty(_myBackingVariable, value))
    ///     return;
    ///     // do something with the new value
    ///     }
    /// </example>
    /// <returns>True if the value was different from the initial value of the storage, or otherwise False.</returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    #endregion
}