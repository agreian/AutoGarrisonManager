using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoGarrisonMissions.MVVM
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Methods

        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var propertyName = GetPropertyName(propertyExpression);
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException();

            var body = propertyExpression.Body as MemberExpression;

            if (body == null)
                throw new ArgumentNullException();

            var property = body.Member as PropertyInfo;

            if (property == null)
                throw new ArgumentNullException();

            return property.Name;
        }

        #endregion
    }
}