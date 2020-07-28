using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GymTrainerWPF.ViewModels
{
    public enum ProcessingState
    {
        Idle,
        Busy
    }

    public abstract class NotifyChangedModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents the method that will handle the System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies any listeners that a property has changed
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// A base class for any/all ViewModels
    /// </summary>
    public class ViewModelBase : NotifyChangedModel
    {
        static DependencyObject _dummyDependencyObject = new DependencyObject();

        //used for static locking (design mode)
        static object _threadLock = new object();

        //used for locking
        protected object _lock = new object();

        //used for defining state
        protected ProcessingState _processingState = ProcessingState.Idle;

        /// <summary>
        /// Gets a simple lock object
        /// </summary>
        protected object Lock
        {
            get { return _lock; }
        }

        /// <summary>
        /// Gets an indicator if we are in design mode
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                lock (_threadLock)
                {
                    return DesignerProperties.GetIsInDesignMode(_dummyDependencyObject);
                }
            }
        }

        /// <summary>
        /// Gets or sets the state of the view model
        /// </summary>
        public ProcessingState ProcessingState
        {
            get { return _processingState; }
            set
            {
                if (_processingState != value)
                {
					_processingState = value;

                    OnPropertyChanged("ProcessingState");
                }
            }
        }

    }
    /// <summary>
    /// Class for command binding
    /// </summary>
    public class CommandHandler : ICommand
    {
        /// <summary>
        /// Action will be invoked
        /// </summary>
        private Action _action;
        /// <summary>
        /// Determines whether the command can execute
        /// </summary>
        private bool _canExecute;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="canExecute">Command can be executed</param>
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }
        /// <summary>
        /// Action will be invoked
        /// </summary>
        /// <param name="action"></param>
        public CommandHandler(Action action)
        {
            _action = action;
        }
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current states
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        public void Execute(object parameter)
        {
            _action();
        }
    }
}
