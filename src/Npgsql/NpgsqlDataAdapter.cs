// created on 1/8/2002 at 23:02
//
// Npgsql.NpgsqlDataAdapter.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Data.Common;
using System.Resources;

namespace Npgsql
{
    /// <summary>
    /// Represents the method that handles the <see cref="Npgsql.NpgsqlDataAdapter.RowUpdated">RowUpdated</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="Npgsql.NpgsqlRowUpdatedEventArgs">NpgsqlRowUpdatedEventArgs</see> that contains the event data.</param>
    public delegate void RowUpdatedEventHandler(Object sender, NpgsqlRowUpdatedEventArgs e);
    
    /// <summary>
    /// Represents the method that handles the <see cref="Npgsql.NpgsqlDataAdapter.RowUpdating">RowUpdating</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="Npgsql.NpgsqlRowUpdatingEventArgs">NpgsqlRowUpdatingEventArgs</see> that contains the event data.</param>
    public delegate void RowUpdatingEventHandler(Object sender, NpgsqlRowUpdatingEventArgs e);
    
    
    public sealed class NpgsqlDataAdapter : DbDataAdapter, IDbDataAdapter
    {

        private NpgsqlCommand 	_selectCommand;
        private NpgsqlCommand		_updateCommand;
        private NpgsqlCommand		_deleteCommand;
        private NpgsqlCommand		_insertCommand;

        private NpgsqlCommandBuilder cmd_builder;

        // Log support
        private static readonly String CLASSNAME = "NpgsqlDataAdapter";
        
        
        public event RowUpdatedEventHandler RowUpdated;
        public event RowUpdatingEventHandler RowUpdating;        

        public NpgsqlDataAdapter()
        {}

        public NpgsqlDataAdapter(NpgsqlCommand selectCommand)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            _selectCommand = selectCommand;
            cmd_builder = new NpgsqlCommandBuilder(this);
        }

        public NpgsqlDataAdapter(String selectCommandText, NpgsqlConnection selectConnection) : this(new NpgsqlCommand(selectCommandText, selectConnection))
        {}

        public NpgsqlDataAdapter(String selectCommandText, String selectConnectionString) : this(selectCommandText, new NpgsqlConnection(selectConnectionString))
        {}


        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
            DataRow dataRow,
            IDbCommand command,
            StatementType statementType,
            DataTableMapping tableMapping
        )
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatedEvent");
            return new NpgsqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);



        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
            DataRow dataRow,
            IDbCommand command,
            StatementType statementType,
            DataTableMapping tableMapping
        )
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateRowUpdatingEvent");
            return new NpgsqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(
            RowUpdatedEventArgs value
        )
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdated");
            //base.OnRowUpdated(value);
            if ((RowUpdated != null) && (value is NpgsqlRowUpdatedEventArgs))
                RowUpdated(this, (NpgsqlRowUpdatedEventArgs) value);

        }

        protected override void OnRowUpdating(
            RowUpdatingEventArgs value
        )
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "OnRowUpdating");
            if ((RowUpdating != null) && (value is NpgsqlRowUpdatingEventArgs))
                RowUpdating(this, (NpgsqlRowUpdatingEventArgs) value);
                        
            /*switch (value.StatementType)
            {
                case StatementType.Insert:
                    value.Command = cmd_builder.GetInsertCommand(value.Row);
                    break;
                case StatementType.Update:
                    value.Command = cmd_builder.GetUpdateCommand(value.Row);
                    break;
                case StatementType.Delete:
                    value.Command = cmd_builder.GetDeleteCommand(value.Row);
                    break;
            }
            DataColumnMappingCollection columnMappings = value.TableMapping.ColumnMappings;
            foreach (IDataParameter parameter in value.Command.Parameters)
            {

                string dsColumnName = parameter.SourceColumn;
                if (columnMappings.Contains(parameter.SourceColumn))
                {
                    DataColumnMapping mapping = columnMappings[parameter.SourceColumn];
                    if (mapping != null)
                    {
                        dsColumnName = mapping.DataSetColumn;
                    }
                }
                DataRowVersion rowVersion = DataRowVersion.Default;
                if (value.StatementType == StatementType.Update)
                    rowVersion = parameter.SourceVersion;
                if (value.StatementType == StatementType.Delete)
                    rowVersion = DataRowVersion.Original;
                parameter.Value = value.Row [dsColumnName, rowVersion];
            }
            value.Row.AcceptChanges ();*/
            
        }

        ITableMappingCollection IDataAdapter.TableMappings
        {
            get
            {
                return TableMappings;
            }
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IDbDataAdapter.DeleteCommand");
                return (NpgsqlCommand) DeleteCommand;
            }

            set
            {
                DeleteCommand = (NpgsqlCommand) value;
            }
        }


        public NpgsqlCommand DeleteCommand
        {
            get
            {
                return _deleteCommand;
            }

            set
            {
                _deleteCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get
            {
                return (NpgsqlCommand) SelectCommand;
            }

            set
            {
                SelectCommand = (NpgsqlCommand) value;
            }
        }


        public NpgsqlCommand SelectCommand
        {
            get
            {
                return _selectCommand;
            }

            set
            {
                _selectCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IDbDataAdapter.UpdateCommand");
                return (NpgsqlCommand) UpdateCommand;
            }

            set
            {
                UpdateCommand = (NpgsqlCommand) value;
            }
        }


        public NpgsqlCommand UpdateCommand
        {
            get
            {
                return _updateCommand;
            }

            set
            {
                _updateCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get
            {
                return (NpgsqlCommand) InsertCommand;
            }

            set
            {
                InsertCommand = (NpgsqlCommand) value;
            }
        }


        public NpgsqlCommand InsertCommand
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "InsertCommand");
                return _insertCommand;
            }

            set
            {
                _insertCommand = value;
            }
        }


    }
}


public class NpgsqlRowUpdatingEventArgs : RowUpdatingEventArgs
{
    public NpgsqlRowUpdatingEventArgs (
        DataRow dataRow,
        IDbCommand command,
        StatementType statementType,
        DataTableMapping tableMapping
    ) : base(dataRow, command, statementType, tableMapping)

    {
    }

}

public class NpgsqlRowUpdatedEventArgs : RowUpdatedEventArgs
{
    public NpgsqlRowUpdatedEventArgs (
        DataRow dataRow,
        IDbCommand command,
        StatementType statementType,
        DataTableMapping tableMapping
    ) : base(dataRow, command, statementType, tableMapping)

    {
    }

}
