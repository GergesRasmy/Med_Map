# 1-Account
a)api/account/register			✅
	register for User (username, email, password, role)

b)api/account/login			✅
	login for Customer or Pharmacy (User)

c)api/account/verifyOtp			✅
	Otp verification for new users

d)api/account/requestNewOtp			✅
	if Otp failed to send, lost or expired user can request new one

# 2-Customer
a)api/customer/customerPublicGet?id=			✅
	gets active (registered) customers public data like (username,id)
	if user isn't registered returns error

b)api/customer/register				✅
	registers new customer for the first time 
	mandatory data to be added (phone number and date of birth)

c)api/customer/update				✅
	update user's existing data 
	every thing is optional here user can update one field at a time

# 3-User
a)api/user/privateGet
	gets user private data (registration state doesn't matter)

# 4-medicine
a)api/medicine/add				✅
	adds new medicine all data is required
	(only admin is authorized to add medicine) : any one can use it now for development and testing

b)api/medicine/order/allMedicine				✅
	retrieve all medicine database with pagination

c)api/medicine/getById?id=				✅
	retrieve medicine by id

d)api/medicine/update				✅
	patches selected fields of medicine
	requires id 
	(only admin is authorized to update medicine) : any one can use it now for development and testing

e)api/medicine/delete?id=				✅
	deletes medicine by id
	(only admin is authorized to delete medicine) : any one can use it now for development and testing

f)api/medicine/search?query= 				✅
	retrieve medicine by name or part of the name  with pagination

# 4-Order 
	user must register to use this controller




# ToDo non used models
customer saved locations
doctor request
recommendation
wallet 
withdrawal request


